# 📊 Repository Pattern - Guia Completo

## Introdução

O **Repository Pattern** abstrai o acesso a dados, permitindo que a aplicação trabalhe com um repositório genérico em vez de SQL direto.

### 🎯 Objetivo

- ✅ Abstrair acesso a dados
- ✅ Facilitar testes (usar mock repository)
- ✅ Permitir trocar implementação (BD diferente)
- ✅ Centralizar queries

---

## 🔍 O Padrão

### Sem Repository

```csharp
// ❌ Acoplado ao EF
public class PlaceService
{
    private readonly PlaceRentalDbContext _context;

    public Place GetById(int id)
    {
        // Lógica de BD misturada no serviço
        return _context.Places
            .Include(p => p.User)
            .SingleOrDefault(p => p.Id == id);
    }
}
```

---

### Com Repository

```csharp
// ✅ Desacoplado
public interface IPlaceRepository
{
    Place GetById(int id);
    List<Place> GetAll();
    void Add(Place place);
    void Update(Place place);
    void Delete(int id);
}

public class PlaceRepository : IPlaceRepository
{
    private readonly PlaceRentalDbContext _context;

    public PlaceRepository(PlaceRentalDbContext context)
    {
        _context = context;
    }

    public Place GetById(int id)
    {
        return _context.Places
            .Include(p => p.User)
            .SingleOrDefault(p => p.Id == id);
    }

    public List<Place> GetAll()
    {
        return _context.Places.ToList();
    }

    public void Add(Place place)
    {
        _context.Places.Add(place);
        _context.SaveChanges();
    }
}
```

---

## 🏗️ Estrutura no Projeto

No PlaceRental, o padrão é implícito (usa DbContext direto), mas pode ser implementado assim:

```csharp
// Interface (Application Layer)
public interface IPlaceRepository
{
    Place GetById(int id);
    List<Place> GetAllAvailable(string search, DateTime startDate, DateTime endDate);
    void Add(Place place);
    void Update(Place place);
    void Delete(int id);
}

// Implementação (Infrastructure Layer)
public class PlaceRepository : IPlaceRepository
{
    private readonly PlaceRentalDbContext _context;

    public PlaceRepository(PlaceRentalDbContext context)
    {
        _context = context;
    }

    public Place GetById(int id)
    {
        return _context.Places.SingleOrDefault(p => p.Id == id);
    }

    public List<Place> GetAllAvailable(
        string search,
        DateTime startDate,
        DateTime endDate)
    {
        return _context.Places
            .Include(p => p.User)
            .Where(p =>
                p.Title.Contains(search) &&
                !p.Books.Any(b => /* conflito de datas */) &&
                !p.IsDeleted)
            .ToList();
    }

    public void Add(Place place)
    {
        _context.Places.Add(place);
        _context.SaveChanges();
    }

    public void Update(Place place)
    {
        _context.Places.Update(place);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var place = _context.Places.Find(id);
        if (place != null)
        {
            place.IsDeleted = true;
            _context.SaveChanges();
        }
    }
}
```

---

## 📋 Operações CRUD

### CREATE

```csharp
public interface IPlaceRepository
{
    void Add(Place place);
}

public class PlaceRepository : IPlaceRepository
{
    public void Add(Place place)
    {
        _context.Places.Add(place);
        _context.SaveChanges();
    }
}
```

---

### READ

```csharp
public interface IPlaceRepository
{
    Place GetById(int id);
    List<Place> GetAll();
    List<Place> GetByTitle(string title);
}

public class PlaceRepository : IPlaceRepository
{
    public Place GetById(int id)
    {
        return _context.Places.SingleOrDefault(p => p.Id == id);
    }

    public List<Place> GetAll()
    {
        return _context.Places.ToList();
    }

    public List<Place> GetByTitle(string title)
    {
        return _context.Places
            .Where(p => p.Title.Contains(title))
            .ToList();
    }
}
```

---

### UPDATE

```csharp
public interface IPlaceRepository
{
    void Update(Place place);
}

public class PlaceRepository : IPlaceRepository
{
    public void Update(Place place)
    {
        _context.Places.Update(place);
        _context.SaveChanges();
    }
}
```

---

### DELETE

```csharp
public interface IPlaceRepository
{
    void Delete(int id);
}

public class PlaceRepository : IPlaceRepository
{
    public void Delete(int id)
    {
        var place = _context.Places.Find(id);
        if (place != null)
        {
            _context.Places.Remove(place);
            _context.SaveChanges();
        }
    }
}
```

---

## 💉 Usando com Dependency Injection

```csharp
// Program.cs
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();

// Uso em Service
public class PlaceService
{
    private readonly IPlaceRepository _repository;

    public PlaceService(IPlaceRepository repository)
    {
        _repository = repository;
    }

    public ResultViewModel<Place> GetById(int id)
    {
        var place = _repository.GetById(id);
        if (place == null)
            return ResultViewModel<Place>.Error(null, "Não encontrado");

        return ResultViewModel<Place>.Success(place);
    }
}
```

---

## 🧪 Testando com Mock

```csharp
[TestClass]
public class PlaceServiceTests
{
    [TestMethod]
    public void GetById_WithValidId_ReturnsPlace()
    {
        // Arrange
        var mockRepository = new Mock<IPlaceRepository>();
        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(new Place { Id = 1, Title = "Casa" });

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

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Interface para abstração
public interface IPlaceRepository { }

// ✅ Injetar repository no service
public PlaceService(IPlaceRepository repository) { }

// ✅ Um repository por entidade
public class PlaceRepository : IPlaceRepository { }
public class UserRepository : IUserRepository { }

// ✅ Queries complexas no repository
public List<Place> GetAllAvailable(...) { }
```

### ❌ DON'T

```csharp
// ❌ Sem interface
public class PlaceRepository { }

// ❌ Um repository para tudo
public class GeneralRepository { }

// ❌ Lógica de negócio no repository
public void CreateAndNotify(Place place) { }

// ❌ Queries no service
public ResultViewModel<List<Place>> GetAll()
{
    return _context.Places.ToList();
}
```

---

## 🎓 Conclusão

Repository Pattern:

- 📦 Abstrai acesso a dados
- 🧪 Facilita testes
- 🔄 Permite trocar BD facilmente
- 📝 Centraliza queries

**No PlaceRental**, pode ser implementado para melhor testabilidade!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
