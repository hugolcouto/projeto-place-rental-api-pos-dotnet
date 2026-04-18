# Configuração por Módulos no PlaceRental

Este documento descreve o padrão de configuração usado no bootstrap da aplicação para registrar dependências em blocos organizados por camada.

No projeto, o registro central acontece no `Program.cs`, mas a responsabilidade de registrar cada grupo de serviços foi extraída para métodos de extensão estáticos:

- `AddApplication()` para a camada de aplicação
- `AddInfrastructure()` para a camada de infraestrutura

Esse formato ajuda a manter o `Program.cs` enxuto e deixa claro onde cada dependência é registrada.

## Fluxo atual

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

O fluxo esperado é:

1. A API chama os métodos de extensão no bootstrap.
2. `ApplicationModule` registra os serviços da camada de aplicação.
3. `InfrastructureModule` registra o `DbContext` e outras dependências técnicas.
4. Os controllers recebem as dependências por construtor.

## Implementação encontrada no projeto

### ApplicationModule

O módulo da aplicação concentra o registro dos serviços de caso de uso.

```csharp
public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddService();

        return services;
    }

    public static IServiceCollection AddService(this IServiceCollection services)
    {
        services.AddScoped<IPlaceService, PlaceService>();

        return services;
    }
}
```

Ponto importante: `AddApplication()` não deve chamar a si mesmo. Ele precisa delegar para `AddService()` para registrar `IPlaceService` com `PlaceService`.

### InfrastructureModule

O módulo de infraestrutura concentra o acesso ao banco e configurações técnicas.

```csharp
public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddData(configuration);

        return services;
    }
}
```

O método interno `AddData()` usa a string de conexão `PlaceRentalCs` e registra o `PlaceRentalDbContext` com `UseSqlServer`.

## Por que esse padrão é útil

- Centraliza a configuração de DI por responsabilidade.
- Facilita leitura do `Program.cs`.
- Evita espalhar `AddScoped`, `AddDbContext` e outras chamadas pelo projeto.
- Torna mais simples adicionar novos serviços por camada.

## Relação com Clean Architecture

Esse padrão combina bem com uma arquitetura em camadas:

- `API` orquestra a inicialização.
- `Application` registra serviços de caso de uso.
- `Infrastructure` registra persistência e integrações técnicas.

Na prática, isso mantém a composição do aplicativo no ponto de entrada e reduz acoplamento entre controllers e implementações concretas.

## Exemplo do que verificar ao adicionar um novo serviço

Quando criar um novo service na camada de aplicação:

1. Criar a interface no projeto `Application`.
2. Criar a implementação concreta.
3. Registrar o contrato no `ApplicationModule` usando `AddScoped`, `AddTransient` ou `AddSingleton`, conforme o ciclo de vida necessário.
4. Injetar a interface no controller ou em outro service.

## Erro comum

O erro mais fácil de introduzir nesse padrão é fazer um método de extensão chamar a si próprio.

Exemplo incorreto:

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddApplication();
    return services;
}
```

Isso gera recursão infinita em tempo de execução. O método correto deve chamar o método responsável pelo registro real, como `AddService()`.

## Resumo

O padrão usado no PlaceRental segue a ideia de composição por módulos: o `Program.cs` chama extensões específicas e cada camada se responsabiliza por registrar seus próprios serviços.

Isso deixa a inicialização mais limpa, facilita manutenção e torna explícito onde cada dependência vive.
