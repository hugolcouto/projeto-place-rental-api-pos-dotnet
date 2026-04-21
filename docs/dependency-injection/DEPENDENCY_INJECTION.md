# Dependency Injection (DI) no PlaceRental

## Objetivo

Neste projeto, DI existe para:

- desacoplar controller, service e repository
- centralizar registro no bootstrap
- permitir troca de implementação por interface
- evitar criação manual de dependências

---

## Como o projeto está configurado hoje

### 1) Composição no Program.cs

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

Esse encadeamento delega os registros para módulos por camada.

### 2) Registros da camada Application

```csharp
public static IServiceCollection AddService(this IServiceCollection services)
{
    services.AddScoped<IPlaceService, PlaceService>();
    services.AddScoped<IUserService, UserService>();

    return services;
}
```

### 3) Registros da camada Infrastructure

```csharp
private static IServiceCollection AddRepositories(this IServiceCollection services)
{
    services.AddScoped<IPlaceRepository, PlaceRepository>();
    services.AddScoped<IUserRepository, UserRepository>();

    return services;
}
```

```csharp
services.AddDbContext<PlaceRentalDbContext>(
    o => o.UseSqlServer(connectionString)
);
```

---

## Fluxo real de resolução

1. O ASP.NET Core cria o controller.
2. O controller pede o service por interface (`IPlaceService`, `IUserService`).
3. O container resolve a implementação concreta (`PlaceService`, `UserService`).
4. O service pede repository por interface (`IPlaceRepository`, `IUserRepository`).
5. O container resolve `PlaceRepository`/`UserRepository` e injeta `PlaceRentalDbContext`.

Se qualquer elo não estiver registrado, a aplicação quebra no startup (ou no primeiro uso, dependendo da validação).

---

## Ciclos de vida usados

### Scoped (usado no projeto)

`Scoped` cria uma instância por requisição HTTP.

No PlaceRental, os principais serviços de negócio e persistência estão como `Scoped`:

- services (`IPlaceService`, `IUserService`)
- repositories (`IPlaceRepository`, `IUserRepository`)
- `DbContext`

Esse conjunto evita inconsistências de estado ao longo da mesma request.

---

## Exemplos de implementação

### Exemplo 1: Injeção correta no service

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
        => _userRepository = userRepository;
}
```

Por que funciona:

- o construtor pede a interface (`IUserRepository`)
- essa interface está registrada no módulo de infraestrutura

### Exemplo 2: Injeção correta no controller

```csharp
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
        => _userService = userService;
}
```

Por que funciona:

- o controller não conhece `UserService` concreto
- depende apenas de contrato

---

## Erros comuns e como interpretar

### Erro 1: Unable to resolve service for type X while attempting to activate Y

Exemplo típico:

```text
Unable to resolve service for type
'PlaceRentalApp.Infrastructure.Persistence.Repositories.UserRepository'
while attempting to activate
'PlaceRentalApp.Application.Services.UserService'.
```

Causa comum:

- construtor pede classe concreta (`UserRepository`)
- registro foi feito para interface (`IUserRepository`)

Como diagnosticar:

1. Veja o tipo pedido no construtor de `Y`.
2. Procure no módulo se esse tipo exato está registrado.
3. Verifique se o contrato pedido corresponde ao contrato registrado.

### Erro 2: Falha em cadeia por dependência indireta

Mesmo com `IUserService` registrado, ele pode falhar ao construir se alguma dependência interna não estiver registrada.

Exemplo:

- `IUserService -> UserService` registrado
- mas `IUserRepository` não registrado
- resolução de `IUserService` quebra

### Erro 3: Recursão em método de extensão

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddApplication(); // recursão infinita
    return services;
}
```

Esse bug não é de container em si, mas aparece durante bootstrap e impede inicialização.

---

## Anti-patterns para evitar

### Service Locator

```csharp
public class PlaceService
{
    public PlaceService(IServiceProvider provider)
    {
        // Evitar: dependências ficam implícitas
    }
}
```

Prefira construtor explícito com contratos.

### Dependência concreta sem necessidade

```csharp
public class UserService
{
    public UserService(UserRepository userRepository) { }
}
```

Prefira interface para reduzir acoplamento e simplificar testes.

---

## Checklist de DI ao criar feature

1. Criou interface e implementação do service?
2. Registrou interface para implementação em `ApplicationModule`?
3. O service injeta contratos, não concretos?
4. Criou interface e implementação de repository?
5. Registrou repository no `InfrastructureModule`?
6. O controller injeta interface do service?
7. O `DbContext` está registrado e com connection string válida?

---

## Resumo

No PlaceRental, DI está organizada por módulos e baseada em contratos. O ponto mais crítico é consistência entre:

- o tipo pedido no construtor
- o tipo registrado no container

Quando esse alinhamento falha, o erro de `Unable to resolve service` aparece imediatamente.
