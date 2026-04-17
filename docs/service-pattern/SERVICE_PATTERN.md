# ⚙️ Service Pattern - Guia Completo

## Introdução

O **Service Pattern** é um padrão que encapsula a lógica de negócio em classes chamadas **Services**. Eles orquestram operações entre múltiplas entidades e repositories.

### 🎯 Objetivo

- ✅ Centralizar lógica de negócio
- ✅ Desacoplar controllers de domínio
- ✅ Reutilizar lógica em múltiplos places
- ✅ Facilitar testes

---

## 📐 O Que é um Service?

Um Service é uma classe que:

1. **Valida** regras de negócio
2. **Coordena** entre múltiplas entidades
3. **Transforma** dados (Entity → ViewModel)
4. **Retorna** ResultViewModel
5. **Não sabe** sobre HTTP, Controllers ou Views

### No PlaceRental

```csharp
public interface IPlaceService
{
    ResultViewModel<PlaceDetailsViewModel?> GetById(int id);
    ResultViewModel<int> Inset(CreatePlaceInputModel model);
    ResultViewModel<List<PlaceViewModel>> GetAllAvailable(
        string search, DateTime startDate, DateTime endDate);
    // ... mais métodos
}

public class PlaceService : IPlaceService
{
    private readonly PlaceRentalDbContext _context;

    public PlaceService(PlaceRentalDbContext context)
    {
        _context = context;
    }

    // Implementação dos métodos
}
```

---

## 🔄 Exemplo: GetById

### O Fluxo

```
Controller
    ↓
    calls _placeService.GetById(1)
    ↓
PlaceService
    ├─ Validar entrada
    ├─ Buscar no banco
    ├─ Validar resultado
    ├─ Transformar para ViewModel
    └─ Retornar ResultViewModel<T>
    ↓
Controller retorna Ok(resultado)
    ↓
Cliente recebe JSON
```

### Implementação

```csharp
public class PlaceService : IPlaceService
{
    private readonly PlaceRentalDbContext _context;

    public PlaceService(PlaceRentalDbContext context)
    {
        _context = context;
    }

    // Implementação do padrão Result
    public ResultViewModel<PlaceDetailsViewModel?> GetById(int id)
    {
        // 1. VALIDAÇÃO
        Place? place = _context.Places.SingleOrDefault(p => p.Id == id);

        // 2. VERIFICAÇÃO
        if (place == null)
            return ResultViewModel<PlaceDetailsViewModel>
                .Error(null, "Local não encontrado");

        // 3. TRANSFORMAÇÃO (Entity → ViewModel)
        var viewModel = PlaceDetailsViewModel.FromEntity(place);

        // 4. RETORNO
        return ResultViewModel<PlaceDetailsViewModel>
            .Success(viewModel);
    }
}
```

---

## 📝 Exemplo: Criar (POST)

```csharp
public class PlaceService : IPlaceService
{
    public ResultViewModel<int> Inset(CreatePlaceInputModel model)
    {
        // 1. VALIDAR entrada
        if (string.IsNullOrEmpty(model.Title))
            return ResultViewModel<int>.Error(0, "Título obrigatório");

        // 2. CRIAR Value Object (endereço)
        var address = new Address(
            model.Address.Street,
            model.Address.Number,
            model.Address.ZipCode,
            model.Address.District,
            model.Address.City,
            model.Address.State,
            model.Address.Country
        );

        // 3. CRIAR entidade
        var place = new Place(
            model.Title,
            model.Description,
            model.DailyPrice,
            address,
            model.AllowedNumberPerson,
            model.AllowPets,
            model.CreatedBy
        );

        // 4. PERSISTIR
        _context.Places.Add(place);
        _context.SaveChanges();

        // 5. RETORNAR
        return ResultViewModel<int>.Success(place.Id);
    }
}
```

---

## 🎯 Responsabilidades do Service

### ✅ O Service DEVE fazer:

```csharp
public class PlaceService
{
    // ✅ Validar regras de negócio
    if (place.DailyPrice <= 0)
        return ResultViewModel.Error("Preço inválido");

    // ✅ Coordenar múltiplas operações
    _context.Places.Add(place);
    _context.PlaceBooks.Add(book);
    _context.SaveChanges();

    // ✅ Transformar dados
    var viewModel = PlaceDetailsViewModel.FromEntity(place);

    // ✅ Retornar ResultViewModel
    return ResultViewModel<PlaceDetailsViewModel>.Success(viewModel);
}
```

---

### ❌ O Service NÃO DEVE fazer:

```csharp
public class PlaceService
{
    // ❌ Conhecer sobre HTTP
    // var request = HttpContext.Request;

    // ❌ Retornar Entities brutas
    // return ResultViewModel<Place>.Success(place);

    // ❌ Acessar Views
    // return View("Index", place);

    // ❌ Lógica UI
    // var json = JsonConvert.SerializeObject(place);
}
```

---

## 🔧 Padrão Service Customizado

### Exemplo: Buscar Disponíveis

```csharp
public ResultViewModel<List<PlaceViewModel>> GetAllAvailable(
    string search,
    DateTime startDate,
    DateTime endDate)
{
    // 1. VALIDAR entrada
    if (startDate >= endDate)
        return ResultViewModel<List<PlaceViewModel>>
            .Error(null, "Data inicial maior que final");

    // 2. BUSCAR com lógica complexa
    List<Place> availablePlaces = _context
        .Places
        .Include(p => p.User)
        .Where(p =>
            // Filtro 1: Título
            p.Title.Contains(search) &&

            // Filtro 2: Não está reservado
            !p.Books.Any(b =>
                (startDate >= b.StartDate && startDate <= b.EndDate) ||
                (endDate >= b.StartDate && endDate <= b.EndDate) ||
                (startDate <= b.StartDate && endDate >= b.EndDate)) &&

            // Filtro 3: Não deletado
            !p.IsDeleted
        )
        .ToList();

    // 3. TRANSFORMAR para ViewModel
    List<PlaceViewModel> model = availablePlaces
        .Select(PlaceViewModel.FromEntity)
        .ToList();

    // 4. RETORNAR
    return ResultViewModel<List<PlaceViewModel>>.Success(model);
}
```

---

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Usar interface
public interface IPlaceService
{
    ResultViewModel<Place> GetById(int id);
}

public class PlaceService : IPlaceService
{
    public ResultViewModel<Place> GetById(int id) { ... }
}

// ✅ Injetar dependências
public PlaceService(PlaceRentalDbContext context)
{
    _context = context;
}

// ✅ Retornar ResultViewModel
return ResultViewModel<Place>.Success(place);

// ✅ Validar antes de operar
if (place == null)
    return ResultViewModel.Error("Não encontrado");
```

### ❌ DON'T

```csharp
// ❌ Não usar interface
public class PlaceService { ... }

// ❌ Criar dependências
public PlaceService()
{
    _context = new PlaceRentalDbContext();
}

// ❌ Retornar Entity diretamente
return ResultViewModel<Place>.Success(place);

// ❌ Sem validação
public ResultViewModel<Place> GetById(int id)
{
    return ResultViewModel<Place>.Success(_context.Places.Find(id));
}
```

---

## 📦 Estrutura do Service

```
PlaceService
├── Constructor com injeção de dependências
│   └── PlaceRentalDbContext
│
├── Métodos GET
│   ├── GetById(int id) → ResultViewModel<T>
│   ├── GetAll() → ResultViewModel<List<T>>
│   └── GetAllAvailable(...) → ResultViewModel<List<T>>
│
├── Métodos CREATE
│   └── Insert(CreatePlaceInputModel) → ResultViewModel<int>
│
├── Métodos UPDATE
│   └── Update(int id, UpdatePlaceInputModel) → ResultViewModel
│
└── Métodos DELETE
    └── Delete(int id) → ResultViewModel
```

---

## 🎓 Conclusão

Services são centrais na arquitetura:

- 🏢 Concentram lógica de negócio
- 🔒 Protegem entidades do domínio
- 🧪 São fáceis de testar
- 🔄 Podem ser reutilizados

**No PlaceRental**, `PlaceService` gerencia toda a lógica relacionada a places!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
