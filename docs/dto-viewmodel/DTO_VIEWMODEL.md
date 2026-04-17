# 🔄 DTO / ViewModel - Guia Completo

## Introdução

**DTO (Data Transfer Object)** e **ViewModel** são objetos especializados para **transferir dados** entre camadas, sem expor entidades de domínio.

### 🎯 Objetivo

- ✅ Separar representação interna de externa
- ✅ Selecionar apenas campos necessários
- ✅ Proteger a entidade de domínio
- ✅ Validar entrada de forma centralizada

---

## 📊 Entity vs DTO vs ViewModel

```
ENTITY (Core Layer)      DTO (Transfer)       VIEWMODEL (Client)
─────────────────────────────────────────────────────────────────
Place                    CreatePlaceInputModel PlaceViewModel
├─ Id                    ├─ Title              ├─ Id
├─ Title                 ├─ Description        ├─ Title
├─ Description           ├─ DailyPrice         ├─ DailyPrice
├─ DailyPrice            ├─ Address            ├─ User.Name
├─ Address (VO)          └─ CreatedBy          └─ Amenities
├─ Books (List)              ↓
├─ Amenities (List)      Validação de          Pronta para
├─ User (FK)             entrada               JSON response
├─ CreatedBy
├─ CreatedAt
└─ IsDeleted

Complexa!                Simples!              Simples!
Com lógica               Sem lógica            Sem lógica
Tudo incluído            Apenas necessário     Apenas necessário
```

---

## 🔄 Tipos de DTOs no PlaceRental

### 1. InputModel - Receber Dados

```csharp
// ✅ Para receber dados de entrada (POST/PUT)
public class CreatePlaceInputModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyPrice { get; set; }
    public CreateAddressModel Address { get; set; }
    public int AllowedNumberPerson { get; set; }
    public bool AllowPets { get; set; }
    public int CreatedBy { get; set; }
}

public class CreateAddressModel
{
    public string Street { get; set; }
    public string Number { get; set; }
    public string ZipCode { get; set; }
    public string District { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}
```

**Uso no Controller**:

```csharp
[HttpPost]
public IActionResult Post(CreatePlaceInputModel model)
{
    // ModelState.IsValid verifica automáticamente
    var result = _placeService.Inset(model);
    return CreatedAtAction(nameof(GetById), new { id = result.Data }, model);
}
```

---

### 2. ViewModel - Retornar Dados

```csharp
// ✅ Para retornar dados (GET)
public class PlaceViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyPrice { get; set; }
    public UserViewModel User { get; set; }
    public List<AmenityViewModel> Amenities { get; set; }

    // ✅ Factory Method para transformar Entity
    public static PlaceViewModel FromEntity(Place place)
    {
        return new PlaceViewModel
        {
            Id = place.Id,
            Title = place.Title,
            Description = place.Description,
            DailyPrice = place.DailyPrice,
            User = UserViewModel.FromEntity(place.User),
            Amenities = place.Amenities
                .Select(AmenityViewModel.FromEntity)
                .ToList()
        };
    }
}
```

**Uso no Controller**:

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var result = _placeService.GetById(id);
    return Ok(result); // Retorna PlaceViewModel em JSON
}
```

---

### 3. DetailViewModel - Mais Dados

```csharp
// ✅ Para retornar dados mais completos
public class PlaceDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyPrice { get; set; }
    public AddressViewModel Address { get; set; }      // Mais detalhes
    public int AllowedNumberPerson { get; set; }
    public bool AllowPets { get; set; }
    public UserViewModel User { get; set; }
    public List<AmenityViewModel> Amenities { get; set; }
    public List<BookViewModel> Books { get; set; }     // Mais dados

    public static PlaceDetailsViewModel FromEntity(Place place)
    {
        return new PlaceDetailsViewModel
        {
            Id = place.Id,
            Title = place.Title,
            // ...
            Address = new AddressViewModel
            {
                Street = place.Address.Street,
                Number = place.Address.Number,
                City = place.Address.City,
                // ...
            },
            Books = place.Books.Select(BookViewModel.FromEntity).ToList()
        };
    }
}

public class AddressViewModel
{
    public string Street { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public string State { get; set; }
}
```

---

## 🔄 Transformação Entity → ViewModel

### Padrão Factory Method

```csharp
// ✅ Usar factory method na entidade ou ViewModel
public class PlaceViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }

    // ✅ Criar a partir de Entity
    public static PlaceViewModel FromEntity(Place place)
    {
        return new PlaceViewModel
        {
            Id = place.Id,
            Title = place.Title,
            // ... mapear outros campos
        };
    }
}

// Uso
var place = _context.Places.Find(1);
var viewModel = PlaceViewModel.FromEntity(place);
```

---

### Usando no Service

```csharp
public class PlaceService
{
    public ResultViewModel<PlaceViewModel> GetById(int id)
    {
        var place = _context.Places.Find(id);

        if (place == null)
            return ResultViewModel<PlaceViewModel>.Error(null, "Não encontrado");

        // ✅ Transformar aqui!
        var viewModel = PlaceViewModel.FromEntity(place);

        return ResultViewModel<PlaceViewModel>.Success(viewModel);
    }
}
```

---

## 🛡️ Proteção de Dados

### ❌ Expor Entity Diretamente

```csharp
// ❌ Péssimo: expõe tudo da entidade
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var place = _context.Places.Find(id);
    return Ok(place); // ❌ Retorna tudo!
}

// JSON retornado expõe:
{
    "id": 1,
    "title": "Casa",
    "createdBy": "admin",  // ❌ Dados sensíveis
    "isDeleted": false,    // ❌ Lógica interna
    "internalNotes": "..." // ❌ Privado!
}
```

---

### ✅ Usar ViewModel

```csharp
// ✅ Bom: apenas dados necessários
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var result = _placeService.GetById(id);
    return Ok(result);
}

// JSON retornado apenas:
{
    "data": {
        "id": 1,
        "title": "Casa",
        "dailyPrice": 150.00
        // ✅ Sem dados sensíveis!
    },
    "isSuccess": true
}
```

---

## 📝 Validação em InputModel

### Com Data Annotations

```csharp
public class CreatePlaceInputModel
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(255, MinimumLength = 5)]
    public string Title { get; set; }

    [Range(1, 10000)]
    public decimal DailyPrice { get; set; }

    [Required]
    public CreateAddressModel Address { get; set; }
}

// Uso no Controller
[HttpPost]
public IActionResult Post(CreatePlaceInputModel model)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState); // ✅ Validação automática

    var result = _placeService.Inset(model);
    return CreatedAtAction(nameof(GetById), new { id = result.Data }, model);
}
```

---

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Criar ViewModel para cada endpoint
public class PlaceViewModel { }
public class PlaceDetailsViewModel { }
public class PlaceListViewModel { }

// ✅ Usar factory method
public static PlaceViewModel FromEntity(Place place) { ... }

// ✅ Retornar ViewModel em APIs
return ResultViewModel<PlaceViewModel>.Success(viewModel);

// ✅ Validar InputModel
[Required, StringLength(255)]
public string Title { get; set; }

// ✅ Proteger dados sensíveis
// Não incluir passwords, tokens, emails no ViewModel
```

### ❌ DON'T

```csharp
// ❌ Retornar Entity diretamente
return Ok(place);

// ❌ Um ViewModel para tudo
public class PlaceDto { } // Para GET, POST, PUT...

// ❌ Sem validação
public string Title { get; set; } // Qualquer valor!

// ❌ Expor dados sensíveis
public string AdminPassword { get; set; }
public string InternalNotes { get; set; }

// ❌ Lógica de negócio em ViewModel
public void CalculatePrice() { } // Isso é responsabilidade da Entity!
```

---

## 🎓 Conclusão

DTO/ViewModel são essenciais:

- 🔒 Protegem entidades de domínio
- 📋 Controlam o contrato da API
- ✅ Validam entrada
- 🧪 Facilitam testes

**No PlaceRental**, ViewModels como `PlaceViewModel`, `CreatePlaceInputModel` definem a interface externa da API!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
