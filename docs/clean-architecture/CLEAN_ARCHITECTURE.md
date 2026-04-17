# 🏗️ Clean Architecture - Guia Completo

## Introdução

**Clean Architecture** é uma abordagem de design que organiza o código em camadas independentes e testáveis, isolando a lógica de negócio das dependências externas (frameworks, bancos de dados, interfaces).

### 🎯 Objetivo Principal

Criar um sistema onde:

- ✅ A lógica de negócio é independente de frameworks
- ✅ Fácil de testar (sem dependências externas)
- ✅ Fácil de manter (mudanças isoladas)
- ✅ Flexível (trocar implementações facilmente)

---

## 📐 Estrutura do PlaceRental

Seu projeto está organizado em **4 camadas principais**:

```
PlaceRentalApp.sln
├── PlaceRentalApp.API           (Presentation Layer)
├── PlaceRentalApp.Application   (Application Layer)
├── PlaceRentalApp.Core          (Domain Layer)
└── PlaceRentalApp.Infrastructure (Infrastructure Layer)
```

### 1️⃣ **Core** (Camada de Domínio - Domain Layer)

**Responsabilidade**: Contém a lógica pura de negócio, independente de qualquer tecnologia.

**Não depende de nada** ➜ É a camada mais importante

```
Core/
├── Entities/          → Objetos de negócio (Place, User, PlaceBook)
├── ValueObjects/      → Valores imutáveis (Address)
├── Enums/             → Enumerações (PlaceStatus)
└── Interfaces/        → Contratos de serviços (opcionalmente)
```

**Características**:

- Sem referências a frameworks
- Sem acesso a banco de dados
- Sem HTTP, sem Controllers
- Apenas lógica pura

**Exemplos no projeto**:

```csharp
// Entities
public class Place : BaseEntity { ... }
public class User : BaseEntity { ... }
public class PlaceBook : BaseEntity { ... }

// Value Objects
public class Address { ... }

// Enums
public enum PlaceStatus { Active, Inactive }
```

---

### 2️⃣ **Application** (Camada de Aplicação)

**Responsabilidade**: Orquestra a lógica de negócio, coordena entre camadas.

**Depende apenas de Core**

```
Application/
├── Services/          → Serviços de negócio (PlaceService)
├── Models/            → ViewModels, DTOs, InputModels
├── Exceptions/        → Exceções customizadas
└── Interfaces/        → Contratos de serviços (IPlaceService)
```

**Características**:

- Implementa casos de uso
- Valida regras de negócio
- Coordena entre múltiplas entidades
- Retorna DTOs (não entidades brutas)

**Exemplos no projeto**:

```csharp
// Service Interface (contrato)
public interface IPlaceService
{
    ResultViewModel<PlaceDetailsViewModel> GetById(int id);
    ResultViewModel<int> Inset(CreatePlaceInputModel model);
}

// DTO/ViewModel
public class CreatePlaceInputModel { ... }
public class PlaceDetailsViewModel { ... }
```

---

### 3️⃣ **Infrastructure** (Camada de Infraestrutura)

**Responsabilidade**: Implementações técnicas, acesso a dados, serviços externos.

**Depende de Application e Core**

```
Infrastructure/
├── Persistence/       → DbContext, Migrations
├── Repositories/      → Implementações de repositórios
├── Services/          → Serviços externos (email, API)
└── Configuration/     → Setup de banco de dados
```

**Características**:

- Implementa acesso a banco de dados
- Conecta a serviços externos
- Traduz modelos de domínio

**Exemplos no projeto**:

```csharp
// DbContext
public class PlaceRentalDbContext : DbContext { ... }

// Migrations
public class FirstMigration : Migration { ... }
```

---

### 4️⃣ **API** (Camada de Apresentação - Presentation Layer)

**Responsabilidade**: Expõe a aplicação via HTTP, recebe requisições, retorna respostas.

**Depende de Application, Infrastructure e Core**

```
API/
├── Controllers/       → Endpoints HTTP
├── Middlewares/       → Processamento de requisição
├── Program.cs         → Configuração da aplicação
└── appsettings.json   → Configurações
```

**Características**:

- Mapeia rotas HTTP
- Valida entrada (ModelState)
- Retorna respostas HTTP
- Trata exceções

**Exemplos no projeto**:

```csharp
// Controller
[Route("api/places")]
public class PlacesController : ControllerBase { ... }

// Middleware
public class ApiExceptionHandler : IExceptionHandler { ... }
```

---

## 🔄 Fluxo de Dados Entre Camadas

```
┌─────────────────────────────────────────────────────────┐
│  Cliente (HTTP Request)                                 │
└──────────────────────┬──────────────────────────────────┘
                       │
        ┌──────────────▼───────────────┐
        │  API Layer (Controllers)     │
        │  - Validar entrada           │
        │  - Mapear para InputModel    │
        └──────────────┬───────────────┘
                       │
        ┌──────────────▼───────────────┐
        │ Application Layer (Services) │
        │ - Validar regras negócio     │
        │ - Orquestrar operações       │
        │ - Retornar ResultViewModel   │
        └──────────────┬───────────────┘
                       │
        ┌──────────────▼───────────────┐
        │ Core Layer (Entities)        │
        │ - Lógica pura de negócio     │
        │ - Sem dependências externas  │
        └──────────────┬───────────────┘
                       │
        ┌──────────────▼────────────────┐
        │ Infrastructure Layer (DB)     │
        │ - Acesso a dados              │
        │ - Entity Framework            │
        └──────────────┬────────────────┘
                       │
        ┌──────────────▼───────────────┐
        │ SQL Server / Banco de Dados  │
        └──────────────────────────────┘
```

---

## 📊 Diagrama de Dependências

```
┌─────────────────────────────────────┐
│           API Layer                 │
│  (Controllers, Middlewares)         │
└──────────────┬──────────────────────┘
               │
               ▼ (depende)
┌──────────────────────────────────────┐
│      Application Layer               │
│  (Services, ViewModels, DTOs)        │
└──────────────┬──────────────────────┘
               │
               ├─────────────┬─────────────┐
               ▼             ▼             ▼
    ┌──────────────┐ ┌────────────┐ ┌─────────────┐
    │ Core Layer   │ │ Infra Layer│ │ Depends on  │
    │(no outside)  │ │(DB, Files) │ │   Core      │
    └──────────────┘ └────────────┘ └─────────────┘
               △                            │
               └────────────────────────────┘
                        (implements)
```

**Regra de Ouro**:

- ✅ Camadas superiores podem depender de camadas inferiores
- ❌ Camadas inferiores NÃO podem depender de camadas superiores
- ✅ Core é completamente independente

---

## 💾 Exemplo: Fluxo Completo de GET /api/places/1

### Passo 1: API Layer (PlacesController.cs)

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    // 1. Validação de entrada (já feita pelo routing)
    // 2. Chamar o serviço
    ResultViewModel<PlaceDetailsViewModel> place =
        _placeService.GetById(id);

    // 3. Retornar resposta HTTP
    return Ok(place);
}
```

**Responsabilidades**:

- ✅ Mapear rota HTTP
- ✅ Extrair parâmetro da URL
- ✅ Chamar serviço
- ✅ Retornar resposta HTTP

---

### Passo 2: Application Layer (PlaceService.cs)

```csharp
public ResultViewModel<PlaceDetailsViewModel?> GetById(int id)
{
    // 1. Validação de negócio
    Place? place = _context.Places.SingleOrDefault(p => p.Id == id);

    if (place == null)
        return ResultViewModel<PlaceDetailsViewModel>.Error(
            null, "Local não encontrado");

    // 2. Transformar entidade em ViewModel
    var viewModel = PlaceDetailsViewModel.FromEntity(place);

    // 3. Retornar resultado
    return ResultViewModel<PlaceDetailsViewModel>.Success(viewModel);
}
```

**Responsabilidades**:

- ✅ Validar regras de negócio
- ✅ Coordenar entre entidades
- ✅ Transformar dados (Entity → ViewModel)
- ✅ Retornar ResultViewModel

---

### Passo 3: Core Layer (Place Entity)

```csharp
public class Place : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal DailyPrice { get; private set; }
    public Address Address { get; private set; }
    public PlaceStatus Status { get; private set; }

    // Lógica de negócio
    public void Update(string title, string description, decimal dailyPrice)
    {
        Title = title;
        Description = description;
        DailyPrice = dailyPrice;
    }
}
```

**Responsabilidades**:

- ✅ Representar conceito de negócio (um imóvel)
- ✅ Encapsular lógica relacionada
- ✅ Não sabe sobre HTTP, DB ou outras camadas

---

### Passo 4: Infrastructure Layer (DbContext)

```csharp
public class PlaceRentalDbContext : DbContext
{
    public DbSet<Place> Places { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Place>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasMany(p => p.Books)
                .WithOne(b => b.Place)
                .HasForeignKey(a => a.IdPlace);
            // ... configurações
        });
    }
}
```

**Responsabilidades**:

- ✅ Mapear entidades para tabelas
- ✅ Configurar relacionamentos
- ✅ Gerenciar acesso a dados

---

## 🎯 Princípios de Clean Architecture

### 1. Inversão de Controle (IoC) / Inversão de Dependência

```csharp
// ❌ Acoplado (sem Clean Architecture)
public class PlaceService
{
    private SqlConnection connection = new SqlConnection();

    public void GetPlace(int id)
    {
        var data = connection.Query("SELECT...");
    }
}

// ✅ Desacoplado (com Clean Architecture)
public class PlaceService
{
    private readonly IPlaceRepository _repository;

    public PlaceService(IPlaceRepository repository)
    {
        _repository = repository;
    }

    public Place GetPlace(int id)
    {
        return _repository.GetById(id);
    }
}
```

### 2. Separação de Responsabilidades

```csharp
// ❌ Muitas responsabilidades
public class PlaceController
{
    public void CreatePlace(Place place)
    {
        // Validar entrada
        // Conectar BD
        // Executar INSERT SQL
        // Enviar email
        // Logar
        // Retornar resposta
    }
}

// ✅ Cada classe tem uma responsabilidade
public class PlacesController : ControllerBase
{
    public IActionResult Post(CreatePlaceInputModel model)
    {
        return Ok(_service.Create(model));
    }
}

public class PlaceService
{
    public ResultViewModel<int> Create(CreatePlaceInputModel model)
    {
        var place = new Place(model.Title, ...);
        _repository.Add(place);
        return ResultViewModel<int>.Success(place.Id);
    }
}
```

### 3. Testabilidade

```csharp
// ✅ Fácil de testar (sem dependências externas)
[TestClass]
public class PlaceServiceTests
{
    [TestMethod]
    public void GetPlace_WithValidId_ReturnsPlace()
    {
        // Arrange
        var mockRepository = new Mock<IPlaceRepository>();
        mockRepository.Setup(r => r.GetById(1))
            .Returns(new Place { Title = "Casa" });

        var service = new PlaceService(mockRepository.Object);

        // Act
        var result = service.GetById(1);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Casa", result.Data.Title);
    }
}
```

---

## ✅ Benefícios da Clean Architecture

| Benefício                       | Descrição                                    |
| ------------------------------- | -------------------------------------------- |
| **Testabilidade**               | Fácil escrever testes unitários              |
| **Manutenibilidade**            | Mudanças isoladas a uma camada               |
| **Escalabilidade**              | Adicionar features sem quebrar existentes    |
| **Reusabilidade**               | Lógica pode ser usada em múltiplos contextos |
| **Independência de Frameworks** | Trocar SQL por MongoDB sem mexer em services |
| **Clareza**                     | Código organizado e fácil de entender        |

---

## 🚀 Boas Práticas

### ✅ DO

```csharp
// ✅ Core depende apenas de si mesmo
public class Place : BaseEntity
{
    public string Title { get; private set; }
}

// ✅ Application depende de Core
public class PlaceService
{
    public ResultViewModel<Place> GetPlace(int id) { ... }
}

// ✅ API orquestra tudo
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    return Ok(_service.GetPlace(id));
}

// ✅ Use interfaces para abstração
public interface IPlaceService { }
public class PlaceService : IPlaceService { }
```

### ❌ DON'T

```csharp
// ❌ Core dependendo de frameworks
using Microsoft.EntityFrameworkCore;
public class Place
{
    public DbSet<PlaceAmenity> Amenities { get; set; }
}

// ❌ Misturar responsabilidades
public class PlaceService
{
    public void CreatePlaceAndSendEmail(CreatePlaceModel model)
    {
        // Criar place
        // Enviar email
        // Registrar log
        // Retornar resposta HTTP
    }
}

// ❌ Controllers acessando banco diretamente
[HttpGet]
public IActionResult GetAll()
{
    return Ok(DbContext.Places.ToList());
}
```

---

## 📚 Conclusão

Clean Architecture garante que seu projeto:

- 📦 Está bem organizado em camadas
- 🧪 É fácil de testar
- 🔄 É flexível e escalável
- 📝 É fácil de manter e entender

**No PlaceRental**, você tem uma estrutura clara que permite crescimento saudável!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
