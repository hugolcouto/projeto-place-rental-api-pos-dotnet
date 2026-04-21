# DI por Modulos no PlaceRental

Este documento mostra como a configuracao de DI esta separada por camada e como depurar falhas de construcao de servicos.

## Estrutura de bootstrap

No ponto de entrada da API:

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

Cada metodo de extensao registra apenas o que pertence a sua camada.

---

## Modulo Application

Responsabilidade: registrar services de caso de uso.

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddService();

    return services;
}

public static IServiceCollection AddService(this IServiceCollection services)
{
    services.AddScoped<IPlaceService, PlaceService>();
    services.AddScoped<IUserService, UserService>();

    return services;
}
```

## Modulo Infrastructure

Responsabilidade: banco, repositories e detalhes tecnicos.

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration
)
{
    services
        .AddData(configuration)
        .AddRepositories();

    return services;
}
```

```csharp
private static IServiceCollection AddRepositories(this IServiceCollection services)
{
    services.AddScoped<IPlaceRepository, PlaceRepository>();
    services.AddScoped<IUserRepository, UserRepository>();

    return services;
}
```

---

## Beneficios praticos dessa organizacao

- Program.cs fica enxuto e legivel.
- Cada camada controla seus proprios contratos.
- Novos services e repositories entram sem poluir bootstrap.
- Debug de DI fica mais previsivel.

---

## Exemplo de adicao de nova feature

Suponha um novo servico `IBookService`.

1. Criar contrato e implementacao na camada Application.
2. Registrar no `ApplicationModule`.
3. Se usar repositorio novo, criar contrato na Core e implementacao na Infrastructure.
4. Registrar repositorio no `InfrastructureModule`.
5. Injetar `IBookService` no controller.

Exemplo de registro:

```csharp
services.AddScoped<IBookService, BookService>();
services.AddScoped<IBookRepository, BookRepository>();
```

---

## Erros comuns nesse modelo

### 1) Contrato nao registrado no modulo correto

Sintoma:

```text
Unable to resolve service for type 'X' while attempting to activate 'Y'
```

Causa:

- esqueceu de registrar `X` em `AddApplication` ou `AddInfrastructure`

### 2) Misturar concreto no construtor e interface no registro

Sintoma:

```text
Unable to resolve service for type 'UserRepository' while attempting to activate 'UserService'
```

Causa:

- construtor pede `UserRepository`
- modulo registrou `IUserRepository`

### 3) Metodo de extensao chamando a si mesmo

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddApplication();
    return services;
}
```

Isso causa recursao infinita.

---

## Checklist rapido antes de rodar

1. Todo service novo foi registrado em `AddApplication`?
2. Todo repository novo foi registrado em `AddRepositories`?
3. Construtores pedem interfaces registradas?
4. `AddInfrastructure(builder.Configuration)` esta no bootstrap?
5. Connection string `PlaceRentalCs` existe no appsettings?

---

## Resumo

O padrao modular de DI do PlaceRental funciona bem quando existe consistencia entre:

- contratos definidos
- construtores
- registros nos modulos

A maioria dos erros em runtime vem de desalinhamento entre esses tres pontos.
