# 📊 Repository Pattern - Guia Prático

## O que é Repository Pattern?

O **Repository Pattern** é um padrão de design que **abstrai o acesso a dados**. Ele funciona como um intermediário entre a **lógica de negócio** e o **banco de dados**, permitindo que sua aplicação não se preocupe diretamente com SQL ou Entity Framework.

### 🎯 Por que usar?

- ✅ **Desacoplamento** - Lógica de negócio separada do acesso a dados
- ✅ **Testabilidade** - Fácil criar mocks para testes unitários
- ✅ **Flexibilidade** - Trocar de banco de dados sem mexer no serviço
- ✅ **Centralização** - Todas as queries em um único lugar
- ✅ **Manutenibilidade** - Código mais organizado e legível

---

## Como Funciona - Fluxo na Prática

```
┌─────────────┐      ┌──────────────┐      ┌────────────────┐      ┌────────────────┐
│ Controller  │ ───▶ │  Service     │ ───▶ │   Repository   │ ───▶ │   DbContext    │
│             │      │              │      │                │      │  (Banco de     │
│ (HTTP)      │      │ (Regras de   │      │ (Queries SQL)  │      │   Dados)       │
└─────────────┘      │  Negócio)    │      │                │      └────────────────┘
                     └──────────────┘      └────────────────┘
```

### 🚫 SEM Repository (ACOPLADO - ❌ NÃO FAÇA)

```csharp
// ❌ Lógica de BD misturada no Controller
[HttpPost]
public IActionResult CreatePlace(CreatePlaceInputModel model)
{
    // Lógica de BD direto no Controller
    var place = new Place(model.Title, model.Description, ...);
    _context.Places.Add(place);  // ❌ DbContext direto!
    _context.SaveChanges();

    return Ok(place.Id);
}
```

**Problemas:**

- Controller conhece detalhes do banco
- Impossível testar sem banco real
- Código duplicado em vários controllers

---

### ✅ COM Repository (DESACOPLADO - ✅ FAÇA)

```csharp
// ✅ Controller limpo - apenas recebe requisição HTTP
[HttpPost]
public IActionResult CreatePlace(CreatePlaceInputModel model)
{
    var result = _placeService.Inset(model);
    return Ok(result);
}

// ✅ Service com lógica de negócio
public class PlaceService : IPlaceService
{
    private readonly IPlaceRepository _placeRepository;

    public PlaceService(IPlaceRepository placeRepository)
        => _placeRepository = placeRepository;

    public ResultViewModel<int> Inset(CreatePlaceInputModel model)
    {
        // Lógica de negócio: transformar model em Entity
        var address = new Address(
            model.Address.Street,
            model.Address.Number,
            // ...
        );

        var place = new Place(
            model.Title,
            model.Description,
            model.DailyPrice,
            address,
            model.AllowedNumberPerson,
            model.AllowPets,
            model.CreatedBy
        );

        // ✅ Delegado ao Repository!
        _placeRepository.Add(place);

        return ResultViewModel<int>.Success(place.Id);
    }
}

// ✅ Repository com lógica de acesso a dados
public class PlaceRepository : IPlaceRepository
{
    private readonly PlaceRentalDbContext _context;

    public PlaceRepository(PlaceRentalDbContext context)
        => _context = context;

    public int Add(Place place)
    {
        _context.Places.Add(place);
        _context.SaveChanges();
        return place.Id;
    }
}
```

---

## 🏗️ Estrutura no Projeto PlaceRental

O projeto implementa Repository Pattern em **3 camadas**:

```
PlaceRentalApp.Core
├── Entities/          (Place, User, PlaceBook, etc.)
└── Repositories/      ← INTERFACES (contrato)
    ├── IPlaceRepository.cs
    └── IUserRepository.cs

PlaceRentalApp.Application
└── Services/          ← LÓGICA DE NEGÓCIO
    ├── IPlaceService.cs
    └── PlaceService.cs

PlaceRentalApp.Infrastructure
└── Persistence/       ← IMPLEMENTAÇÃO
    ├── PlaceRentalDbContext.cs
    └── Repositories/
        └── PlaceRepository.cs
```

---

## 📋 Interface do Repository (Contrato)

**Arquivo:** `PlaceRentalApp.Core/Repositories/IPlaceRepository.cs`

```csharp
public interface IPlaceRepository
{
    // READ
    Place? GetById(int id);
    List<Place>? GetAllAvailable(string search, DateTime startDate, DateTime endDate);

    // CREATE
    int Add(Place place);
    void AddBook(PlaceBook book);
    void AddAmenity(PlaceAmenity amenity);

    // UPDATE
    void Update(Place place);

    // DELETE
    void Delete(Place place);
}
```

**Por que uma interface?**

- Define o contrato (quais métodos existem)
- Permite injetar dependência
- Facilita criar mocks para testes

---

## 🔧 Implementação do Repository

**Arquivo:** `PlaceRentalApp.Infrastructure/Persistence/Repositories/PlaceRepository.cs`

```csharp
public class PlaceRepository : IPlaceRepository
{
    private readonly PlaceRentalDbContext _context;

    public PlaceRepository(PlaceRentalDbContext context)
        => _context = context;

    // CREATE
    public int Add(Place place)
    {
        _context.Places.Add(place);
        _context.SaveChanges();
        return place.Id;
    }

    public void AddBook(PlaceBook book)
    {
        _context.PlaceBooks.Add(book);
        _context.SaveChanges();
    }

    public void AddAmenity(PlaceAmenity amenity)
    {
        _context.PlaceAmenities.Add(amenity);
        _context.SaveChanges();
    }

    // READ - Buscar por ID com relacionamentos
    public Place? GetById(int id)
    {
        Place? place = _context
            .Places
            .Include(p => p.Amenities)    // Carrega amenidades
            .Include(p => p.User)          // Carrega usuário
            .SingleOrDefault(p => p.Id == id);

        return place;
    }

    // READ - Buscar disponibilidades (query complexa!)
    public List<Place>? GetAllAvailable(string search, DateTime startDate, DateTime endDate)
    {
        List<Place> availablePlaces = _context
            .Places
            .Include(p => p.User)
            .Where(p =>
                // Contém o termo de busca
                p.Title.Contains(search) &&
                // Não tem reservas que conflitem com datas
                !p.Books.Any(b =>
                    (startDate >= b.StartDate && startDate <= b.EndDate) ||
                    (endDate >= b.StartDate && endDate <= b.EndDate) ||
                    (startDate <= b.StartDate && endDate >= b.EndDate)
                ) &&
                // Não está deletado
                !p.IsDeleted
            )
            .ToList();

        return availablePlaces;
    }

    // UPDATE
    public void Update(Place place)
    {
        _context.Places.Update(place);
        _context.SaveChanges();
    }

    // DELETE (soft delete)
    public void Delete(Place place)
    {
        _context.Places.Update(place);  // Marca como deletado
        _context.SaveChanges();
    }
}
```

**Pontos importantes:**

- Todas as queries SQL estão aqui (`.Where()`, `.Include()`, etc)
- `SaveChanges()` persiste no BD
- O Service não conhece EntityFramework
- Fácil de testar com mock

---

## 💉 Dependency Injection - Como Registrar

### 1️⃣ Registrar Repository em `InfrastructureModule.cs`

**Arquivo:** `PlaceRentalApp.Infrastructure/InfrastructureModule.cs`

```csharp
public static class InfrastructureModule
{
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

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IPlaceRepository, PlaceRepository>();
        // Adicionar o de User
        // services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
```

**O que significa `AddScoped`?**

- **Uma instância por requisição HTTP**
- Ideal para trabalhar com banco de dados
- Cada HTTP request cria/usa uma única instância

---

### 2️⃣ Registrar Service em `ApplicationModule.cs`

**Arquivo:** `PlaceRentalApp.Application/ApplicationModule.cs`

```csharp
public static class ApplicationModule
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddService();
        return services;
    }

    private static IServiceCollection AddService(
        this IServiceCollection services)
    {
        services.AddScoped<IPlaceService, PlaceService>();
        return services;
    }
}
```

---

### 3️⃣ Ativar os Módulos em `Program.cs`

**Arquivo:** `PlaceRentalApp.API/Program.cs`

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ✅ Ativa a injeção de dependência
builder.Services
    .AddApplication()           // Registra PlaceService
    .AddInfrastructure(builder.Configuration);  // Registra PlaceRepository

// ... resto do código
```

---

### 4️⃣ Usar no Controller

**Arquivo:** `PlaceRentalApp.API/Controllers/PlacesController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly IPlaceService _placeService;

    // ✅ Service é injetado automaticamente
    public PlacesController(IPlaceService placeService)
        => _placeService = placeService;

    [HttpPost]
    public IActionResult CreatePlace(CreatePlaceInputModel model)
    {
        // ✅ Service usa Repository internamente
        var result = _placeService.Inset(model);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetPlaceById(int id)
    {
        var result = _placeService.GetById(id);
        return Ok(result);
    }
}
```

**Fluxo completo:**

```
1. Controller pede IPlaceService
   ↓
2. DI Container injetar PlaceService
   ↓
3. PlaceService pede IPlaceRepository
   ↓
4. DI Container injetar PlaceRepository
   ↓
5. PlaceRepository acessa DbContext
   ↓
6. Query executada no banco
```

---

## 🧪 Testando com Mock Repository

A grande vantagem do Repository Pattern é **testar sem banco de dados real**!

```csharp
[TestClass]
public class PlaceServiceTests
{
    [TestMethod]
    public void Inset_WithValidPlace_ReturnsSuccessWithId()
    {
        // 1. ARRANGE - Preparar mock e dados
        var mockRepository = new Mock<IPlaceRepository>();
        mockRepository
            .Setup(r => r.Add(It.IsAny<Place>()))
            .Callback<Place>(p => p.Id = 1);  // Simula ID gerado

        var service = new PlaceService(mockRepository.Object);

        var address = new Address("Rua X", 123, "12345", "Dist", "City", "SP", "BR");
        var model = new CreatePlaceInputModel
        {
            Title = "Casa Bonita",
            Description = "Descrição",
            DailyPrice = 100,
            Address = address,
            AllowedNumberPerson = 4,
            AllowPets = true,
            CreatedBy = 1
        };

        // 2. ACT - Executar ação
        var result = service.Inset(model);

        // 3. ASSERT - Verificar resultado
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data);
        mockRepository.Verify(r => r.Add(It.IsAny<Place>()), Times.Once);
    }

    [TestMethod]
    public void GetById_WithValidId_ReturnsPlace()
    {
        // 1. ARRANGE
        var place = new Place("Casa", "Desc", 100,
            new Address("Rua", 1, "12345", "Dist", "City", "SP", "BR"),
            4, true, 1);
        place.Id = 1;

        var mockRepository = new Mock<IPlaceRepository>();
        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(place);

        var service = new PlaceService(mockRepository.Object);

        // 2. ACT
        var result = service.GetById(1);

        // 3. ASSERT
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Casa", result.Data.Title);
    }
}
```

**Benefícios:**

- ✅ Testa sem banco de dados real
- ✅ Testa mais rápido (mock é instantâneo)
- ✅ Isola a lógica do serviço
- ✅ Controla respostas do repositório

---

## ✅ Boas Práticas

### ✅ FAZER (DO's)

```csharp
// ✅ 1. Sempre use interfaces para repositories
public interface IPlaceRepository
{
    Place? GetById(int id);
    int Add(Place place);
}

// ✅ 2. Um repository por entidade principal
public class PlaceRepository : IPlaceRepository { }
public class UserRepository : IUserRepository { }
public class BookRepository : IBookRepository { }

// ✅ 3. Injetar repository no serviço (não no controller!)
public class PlaceService
{
    private readonly IPlaceRepository _repository;

    public PlaceService(IPlaceRepository repository)
        => _repository = repository;
}

// ✅ 4. Colocar queries complexas no repository
public List<Place>? GetAllAvailable(string search, DateTime startDate, DateTime endDate)
{
    return _context
        .Places
        .Include(p => p.User)
        .Where(p =>
            p.Title.Contains(search) &&
            !p.Books.Any(b => /* lógica complexa */ ) &&
            !p.IsDeleted
        )
        .ToList();
}

// ✅ 5. Usar Result Pattern para retornos
public ResultViewModel<int> Inset(CreatePlaceInputModel model)
{
    var place = new Place(...);
    var id = _repository.Add(place);
    return ResultViewModel<int>.Success(id);
}
```

### ❌ NÃO FAZER (DON'Ts)

```csharp
// ❌ 1. NÃO use DbContext direto no Service/Controller
public IActionResult CreatePlace(CreatePlaceInputModel model)
{
    _context.Places.Add(new Place(...));  // ❌ ERRADO!
    _context.SaveChanges();
}

// ❌ 2. NÃO faça queries no Service
public class PlaceService
{
    public List<Place> Search(string term)
    {
        return _context.Places  // ❌ ERRADO!
            .Where(p => p.Title.Contains(term))
            .ToList();
    }
}

// ❌ 3. NÃO crie repository genérico para tudo
public class GenericRepository<T> { }  // ❌ Muito abstrato!

// ❌ 4. NÃO retorne DbSet ou IQueryable do repository
public IQueryable<Place> GetAll()  // ❌ Expõe EF!
{
    return _context.Places;
}

// ❌ 5. NÃO misture lógica de negócio com acesso a dados
public void CreateAndNotifyPlaceAdded(Place place)
{
    _context.Places.Add(place);
    _context.SaveChanges();
    SendEmail(place.User.Email);  // ❌ Misturado!
}
```

---

## 📊 Resumo: Benefícios Práticos

| Benefício            | Sem Repository     | Com Repository         |
| -------------------- | ------------------ | ---------------------- |
| **Testar Serviço**   | Precisa de BD real | Mock em segundos       |
| **Trocar BD**        | Refatorar tudo     | Trocar 1 implementação |
| **Entender Código**  | Queries espalhadas | Tudo centralizado      |
| **Reutilizar Query** | Copiar/colar       | Chamar método          |
| **Manutenibilidade** | Difícil            | Fácil                  |

---

## 🎯 Conclusão

O **Repository Pattern** é essencial em aplicações profissionais porque:

- 📦 **Abstração** - Separa acesso a dados de lógica de negócio
- 🧪 **Testabilidade** - Testa sem banco real
- 🔄 **Flexibilidade** - Trocar BD sem quebrar serviço
- 📝 **Organização** - Código estruturado e legível
- 🚀 **Performance** - Queries otimizadas em um único lugar

**No PlaceRental**, o padrão permite que:

- PlaceService foque em regras de negócio
- PlaceRepository foque em queries SQL/EF
- Controllers foquem em receber/retornar HTTP

---

**Documento atualizado em**: 21 de Abril de 2026  
**Versão**: 2.0  
**Status**: ✅ Completo com exemplos reais do projeto
