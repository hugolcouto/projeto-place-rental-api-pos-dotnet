# 💉 Dependency Injection - Guia Completo

## Introdução

**Dependency Injection (DI)** é um padrão de design que promove a inversão de controle (IoC), permitindo que objetos recebam suas dependências de forma externa em vez de criá-las internamente.

### 🎯 Objetivo

- ✅ Desacoplar classes
- ✅ Facilitar testes
- ✅ Centralizar configuração
- ✅ Promover reusabilidade

---

## 📚 Conceitos Básicos

### O Problema (Sem DI)

```csharp
// ❌ Acoplado
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;

    public PlaceService()
    {
        // Cria sua própria dependência
        _context = new PlaceRentalDbContext();
    }

    public Place GetPlace(int id)
    {
        return _context.Places.Find(id);
    }
}

// Uso
var service = new PlaceService(); // Acoplado ao DbContext
```

**Problemas**:

- ❌ Impossível testar (não pode usar mock DbContext)
- ❌ Acoplado a implementação concreta
- ❌ Difícil trocar implementação

---

### A Solução (Com DI)

```csharp
// ✅ Desacoplado
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;

    // Dependência é INJETADA
    public PlaceService(PlaceRentalDbContext context)
    {
        _context = context;
    }

    public Place GetPlace(int id)
    {
        return _context.Places.Find(id);
    }
}

// Uso
var context = new PlaceRentalDbContext();
var service = new PlaceService(context); // Injetar dependência
```

**Vantagens**:

- ✅ Fácil de testar (pode usar mock)
- ✅ Desacoplado
- ✅ Flexível

---

## 🔄 Tipos de Injeção

### 1. Constructor Injection (Recomendado)

```csharp
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;

    // ✅ MELHOR: Dependência através do construtor
    public PlaceService(PlaceRentalDbContext context)
    {
        _context = context; // Armazenar para usar depois
    }
}

// Uso
var service = new PlaceService(dbContext);
```

**Vantagens**:

- ✅ Dependências óbvias
- ✅ Imutáveis (readonly)
- ✅ Fácil de testar

---

### 2. Method Injection

```csharp
public class PlaceService
{
    // ✅ Dependência através do método
    public Place GetPlace(int id, PlaceRentalDbContext context)
    {
        return context.Places.Find(id);
    }
}

// Uso
var place = service.GetPlace(1, dbContext);
```

**Quando usar**: Método precisa de diferentes dependências

---

### 3. Property Injection

```csharp
public class PlaceService
{
    // ✅ Dependência através de propriedade
    public PlaceRentalDbContext Context { get; set; }

    public Place GetPlace(int id)
    {
        return Context.Places.Find(id);
    }
}

// Uso
var service = new PlaceService { Context = dbContext };
```

**Evitar**: Menos seguro que Constructor Injection

---

## ⏱️ Ciclos de Vida

Quando registrar uma dependência, você define **como e quando** ela será criada.

### 1. Transient (Sem Memória)

```csharp
// Registrar
builder.Services.AddTransient<IPlaceService, PlaceService>();

// Resultado: Nova instância toda vez!
var service1 = serviceProvider.GetService<IPlaceService>();
var service2 = serviceProvider.GetService<IPlaceService>();

// service1 ≠ service2 (objetos diferentes)
```

**Quando usar**:

- ✅ Serviços stateless (sem estado)
- ✅ Serviços leves
- ❌ Nunca com DbContext!

---

### 2. Scoped (Uma por Request)

```csharp
// Registrar
builder.Services.AddScoped<IPlaceService, PlaceService>();

// Resultado: Mesma instância durante a requisição HTTP
```

**Ciclo**:

```
Requisição HTTP inicia
    ↓
DI cria instância do serviço
    ↓
Serviço é usado no controller
    ↓
Requisição termina
    ↓
Instância é descartada
    ↓
Próxima requisição = nova instância
```

**Quando usar**:

- ✅ **DbContext** (principal uso!)
- ✅ Serviços específicos de request
- ✅ Operações de banco de dados

---

### 3. Singleton (Uma para Sempre)

```csharp
// Registrar
builder.Services.AddSingleton<ICacheService, CacheService>();

// Resultado: Mesma instância para toda a aplicação!
var service1 = serviceProvider.GetService<ICacheService>();
var service2 = serviceProvider.GetService<ICacheService>();

// service1 == service2 (mesmo objeto)
```

**Quando usar**:

- ✅ Cache
- ✅ Configurações
- ✅ Logger
- ❌ Nunca com DbContext! (thread-unsafe)

---

## 📊 Comparação Visual

```
┌─────────────┬──────────────────────┬──────────────────┐
│ Ciclo       │ Instâncias Criadas   │ Quando Usar      │
├─────────────┼──────────────────────┼──────────────────┤
│ Transient   │ 1 por resolução      │ Serviços leves   │
│ Scoped      │ 1 por request HTTP   │ DbContext        │
│ Singleton   │ 1 para sempre        │ Cache, Config    │
└─────────────┴──────────────────────┴──────────────────┘
```

---

## 🔧 No Projeto PlaceRental

### Configuração (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Registrar DbContext (Scoped)
builder.Services.AddScoped<IPlaceService, PlaceService>();

// 2. Registrar Services
builder.Services.AddDbContext<PlaceRentalDbContext>(
    o => o.UseSqlServer(connectionString));

// 3. Registrar Middleware
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
```

### Uso no Controller

```csharp
[ApiController]
[Route("api/places")]
public class PlacesController : ControllerBase
{
    // ✅ DI automático: Framework injeta IPlaceService
    public PlacesController(IPlaceService placeService)
    {
        _placeService = placeService;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _placeService.GetById(id);
        return Ok(result);
    }
}
```

---

## 🔑 Conceitos-Chave

### Service Locator Anti-Pattern

```csharp
// ❌ NÃO FAÇA
public class PlaceService
{
    private ServiceProvider _serviceProvider;

    public PlaceService(ServiceProvider sp)
    {
        _serviceProvider = sp; // Oculta dependências!
    }

    public void SomeMethod()
    {
        var context = _serviceProvider.GetService<PlaceRentalDbContext>();
    }
}

// ✅ FAÇA
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;

    public PlaceService(PlaceRentalDbContext context)
    {
        _context = context; // Dependência explícita
    }

    public void SomeMethod()
    {
        // Usar _context
    }
}
```

---

### Registrar Interface vs Implementação

```csharp
// ✅ RECOMENDADO: Interface + Implementação
builder.Services.AddScoped<IPlaceService, PlaceService>();

// Uso
public PlacesController(IPlaceService placeService) { }

// ✅ Também funciona: Apenas implementação
builder.Services.AddScoped<PlaceService>();

// Uso
public PlacesController(PlaceService placeService) { }

// ❌ EVITAR: Factory
builder.Services.AddScoped(sp => new PlaceService(sp.GetService<DbContext>()));
```

---

## 📚 Padrões Comuns

### 1. Injeção de Múltiplas Dependências

```csharp
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;
    private readonly ILogger<PlaceService> _logger;
    private readonly IEmailService _emailService;

    // ✅ Múltiplas dependências
    public PlaceService(
        PlaceRentalDbContext context,
        ILogger<PlaceService> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }
}
```

---

### 2. Implementação com Factory

```csharp
// ✅ Factory registration
builder.Services.AddScoped<IPlaceService>(sp =>
    new PlaceService(
        sp.GetRequiredService<PlaceRentalDbContext>(),
        sp.GetRequiredService<ILogger<PlaceService>>()
    )
);
```

---

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Constructor injection
public PlaceService(PlaceRentalDbContext context)
{
    _context = context;
}

// ✅ Dependências explícitas
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;
    private readonly ILogger<PlaceService> _logger;
}

// ✅ Usar interfaces
builder.Services.AddScoped<IPlaceService, PlaceService>();

// ✅ DbContext com Scoped
builder.Services.AddDbContext<PlaceRentalDbContext>();
```

### ❌ DON'T

```csharp
// ❌ Service Locator
public PlaceService(IServiceProvider sp) { }

// ❌ Criar dependências internamente
public PlaceService()
{
    _context = new PlaceRentalDbContext();
}

// ❌ Property injection sem necessidade
public ILogger Logger { get; set; }

// ❌ Singleton com DbContext
builder.Services.AddSingleton<PlaceRentalDbContext>();
```

---

## 🎓 Conclusão

Dependency Injection é fundamental em .NET moderno:

- 📦 Desacopla componentes
- 🧪 Facilita testes
- 🔧 Centraliza configuração
- 📈 Melhora manutenibilidade

**No PlaceRental**, DI é configurado no `Program.cs` e usado em todos os Controllers e Services!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
