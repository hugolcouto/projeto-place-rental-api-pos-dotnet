# 🔥 Result Pattern - Exemplos Práticos Avançados

## 📦 Casos de Uso do Projeto PlaceRental

Este documento mostra exemplos práticos e reais extraídos diretamente do seu projeto.

---

## Case 1: Buscar Locais Disponíveis (Lista com Filtros)

**Cenário**: Usuário quer buscar locais disponíveis em um período específico

### Fluxo Detalhado

```
┌──────────────────────────────────┐
│  Request GET /api/places         │
│  ?search=casa                    │
│  &startDate=2024-06-01           │
│  &endDate=2024-06-10             │
└────────────────┬─────────────────┘
                 │
      ┌──────────▼──────────┐
      │ PlacesController    │
      │ Get(search, dates)  │
      └──────────┬──────────┘
                 │
      ┌──────────▼──────────────────────────┐
      │ PlaceService.GetAllAvailable()      │
      │                                     │
      │ 1. Filtrar por título (search)      │
      │ 2. Filtrar por datas disponíveis    │
      │ 3. Excluir deletados                │
      │ 4. Mapear para ViewModel            │
      │ 5. Retornar ResultViewModel<List>   │
      └──────────┬──────────────────────────┘
                 │
      ┌──────────▼──────────────────────────┐
      │ ResultViewModel<List<PlaceViewModel>>│
      │ .Success([{...}, {...}, {...}])     │
      └──────────┬──────────────────────────┘
                 │
      ┌──────────▼──────────────────────────┐
      │ HTTP 200 OK                         │
      │                                     │
      │ {                                   │
      │   "data": [                         │
      │     {                               │
      │       "id": 1,                      │
      │       "title": "Casa na Praia",     │
      │       "dailyPrice": 150.00,         │
      │       "user": { ... }               │
      │     },                              │
      │     {                               │
      │       "id": 3,                      │
      │       "title": "Apartamento Centro",│
      │       "dailyPrice": 100.00,         │
      │       "user": { ... }               │
      │     }                               │
      │   ],                                │
      │   "message": "",                    │
      │   "isSuccess": true                 │
      │ }                                   │
      └────────────────────────────────────┘
```

### Código Real do Projeto

```csharp
// PlaceService.cs
public ResultViewModel<List<PlaceViewModel>> GetAllAvailable(
    string search,
    DateTime startDate,
    DateTime endDate)
{
    List<Place> availablePlaces = _context
        .Places
        .Include(p => p.User)
        .Where(p =>
            // Filtro 1: Busca por título
            p.Title.Contains(search) &&

            // Filtro 2: Verifica se não está reservado nas datas
            !p.Books.Any(b =>
                (startDate >= b.StartDate && startDate <= b.EndDate) ||
                (endDate >= b.StartDate && endDate <= b.EndDate) ||
                (startDate <= b.StartDate && endDate >= b.EndDate))

            // Filtro 3: Exclui deletados
            && !p.IsDeleted
        )
        .ToList();

    // Mapear para ViewModel
    List<PlaceViewModel>? model = availablePlaces
        .Select(PlaceViewModel.FromEntity)
        .ToList()!;

    // Sempre retorna sucesso (mesmo que lista vazia)
    return ResultViewModel<List<PlaceViewModel>>.Success(model);
}
```

```csharp
// PlacesController.cs
[HttpGet]
public IActionResult Get(string search, DateTime startDate, DateTime endDate)
{
    ResultViewModel<List<PlaceViewModel>> availablePlaces =
        _placeService.GetAllAvailable(search, startDate, endDate);

    return Ok(availablePlaces);
}
```

### Possíveis Respostas

**Caso 1: Com resultados**

```json
{
  "data": [
    {
      "id": 1,
      "title": "Casa na Praia",
      "dailyPrice": 150.0
    }
  ],
  "message": "",
  "isSuccess": true
}
```

**Caso 2: Nenhum resultado**

```json
{
  "data": [],
  "message": "",
  "isSuccess": true
}
```

> ⚠️ **Nota importante**: A lista vazia ainda retorna `isSuccess: true` porque a operação foi bem-sucedida. A ausência de dados não é uma falha!

---

## Case 2: Criar Local (Com Validação)

**Cenário**: Usuário cria um novo local para alugar

### Fluxo Completo

```
┌────────────────────────┐
│ POST /api/places       │
│ Content-Type: json     │
│                        │
│ {                      │
│   "title": "...",      │
│   "description": "...",│
│   "dailyPrice": 100,   │
│   "address": {...},    │
│   ...                  │
│ }                      │
└────────────┬───────────┘
             │
   ┌─────────▼──────────┐
   │ Validar entrada    │
   │ (Model State)      │
   └─────────┬──────────┘
             │
   ┌─────────▼─────────────────┐
   │ PlaceService.Insert()     │
   │                           │
   │ 1. Criar entidade         │
   │ 2. Vincular valor         │
   │ 3. Adicionar ao contexto  │
   │ 4. Salvar database        │
   └─────────┬─────────────────┘
             │
   ┌─────────▼──────────────────┐
   │ ResultViewModel<int>       │
   │ .Success(place.Id)         │
   │ { data: 42, ... }          │
   └─────────┬──────────────────┘
             │
   ┌─────────▼──────────────────┐
   │ HTTP 201 Created           │
   │ Location: /api/places/42   │
   │ Body: { data: 42, ... }    │
   └────────────────────────────┘
```

### Código Real

```csharp
// PlaceService.cs
public ResultViewModel<int> Inset(CreatePlaceInputModel model)
{
    // Criar value object de endereço
    var address = new Address(
        model.Address.Street,
        model.Address.Number,
        model.Address.ZipCode,
        model.Address.District,
        model.Address.City,
        model.Address.State,
        model.Address.Country
    );

    // Criar entidade Place
    var place = new Place(
        model.Title,
        model.Description,
        model.DailyPrice,
        address,
        model.AllowedNumberPerson,
        model.AllowPets,
        model.CreatedBy
    );

    // Adicionar e salvar
    _context.Places.Add(place);
    _context.SaveChanges();

    // Retornar o ID do novo local
    return ResultViewModel<int>.Success(place.Id);
}
```

```csharp
// PlacesController.cs
[HttpPost]
public IActionResult Post(CreatePlaceInputModel model)
{
    // Serviço retorna int (ID do local)
    var PlaceId = _placeService.Inset(model);

    // Usar CreatedAtAction para retornar 201 com Location header
    return CreatedAtAction(
        nameof(GetById),
        new { id = PlaceId },
        model
    );
}
```

### Respostas Possíveis

**Sucesso (201 Created)**

```
Status: 201 Created
Location: /api/places/42

{
  "data": 42,
  "message": "",
  "isSuccess": true
}
```

**Falha de Validação (400 Bad Request)**

```
Status: 400 Bad Request

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "...",
  "errors": {
    "title": ["O título é obrigatório"]
  }
}
```

---

## Case 3: Adicionar Amenidade a Local (Com Validação)

**Cenário**: Host quer adicionar uma amenidade (WiFi, piscina, etc) ao seu local

### Diagrama de Estados

```
┌─────────────────────────────┐
│ POST /api/places/1/amenities│
│                             │
│ { description: "WiFi" }     │
└──────────────┬──────────────┘
               │
   ┌───────────▼──────────────┐
   │ PlaceService             │
   │ InsertAmenity(id, model) │
   │                          │
   │ Step 1: Verificar Place  │
   └───────────┬──────────────┘
               │
       ┌───────┴────────┐
       │                │
   ┌───▼─────┐      ┌───▼──────────┐
   │ Existe  │      │ Não Existe   │
   └───┬─────┘      └───┬──────────┘
       │                │
   ┌───▼──────────┐ ┌───▼────────────────┐
   │ Criar        │ │ ResultViewModel<int>│
   │ Amenity      │ │ .Error(null,        │
   │              │ │  "Not Found")       │
   │ Step 2:      │ └───┬────────────────┘
   │ Adicionar DB │    │
   │              │ ┌───▼────────────────┐
   │ Step 3:      │ │ HTTP 200 OK        │
   │ Retornar ID  │ │ {                  │
   │              │ │ "data": null,      │
   │              │ │ "message": "...",  │
   │              │ │ "isSuccess": false │
   │              │ │ }                  │
   └───┬──────────┘ └────────────────────┘
       │
   ┌───▼────────────────┐
   │ ResultViewModel<int>│
   │ .Success(id)       │
   └───┬────────────────┘
       │
   ┌───▼──────────┐
   │ HTTP 201     │
   │ Location: ./ │
   │ Data: 5      │
   └──────────────┘
```

### Código Real

```csharp
// PlaceService.cs
public ResultViewModel<int> InsertAmenity(
    int id,
    CreatePlaceAmenityInputModel model)
{
    // Validação: Place existe?
    bool exists = _context.Places.Any(p => p.Id == id);

    if (!exists)
    {
        return (ResultViewModel<int>)
            ResultViewModel.Error("Local não encontrado");
    }

    // Criar amenidade
    PlaceAmenity amenity = new PlaceAmenity(
        model.Description,
        id
    );

    // Adicionar e salvar
    _context.PlaceAmenities.Add(amenity);
    _context.SaveChanges();

    // Retornar ID da amenidade criada
    return ResultViewModel<int>.Success(amenity.Id);
}
```

```csharp
// PlacesController.cs
[HttpPost("{id}/amenities")]
public IActionResult PostAmenity(
    int id,
    CreatePlaceAmenityInputModel model)
{
    ResultViewModel<int> placeAmenityId =
        _placeService.InsertAmenity(id, model);

    return CreatedAtAction(
        nameof(GetById),
        new { id = placeAmenityId.Data },
        model
    );
}
```

### Respostas

**Sucesso (201 Created)**

```json
{
  "data": 5,
  "message": "",
  "isSuccess": true
}
```

**Falha - Place não existe (200 OK com flag)**

```json
{
  "data": null,
  "message": "Local não encontrado",
  "isSuccess": false
}
```

---

## Case 4: Criar Reserva (Sem Retorno de Dados)

**Cenário**: Usuário faz uma reserva em um local

### Fluxo Simples

```
POST /api/places/1/books
{
  "idUser": 10,
  "idPlace": 1,
  "startDate": "2024-06-01",
  "endDate": "2024-06-10",
  "comments": "Família de 4 pessoas"
}
│
▼
PlaceService.Book()
│
├─ Verificar se Place existe
│
├─ Se não existe:
│  └─ Retornar ResultViewModel.Error()
│
└─ Se existe:
   ├─ Criar PlaceBook
   ├─ Salvar database
   └─ Retornar ResultViewModel.Success()
│
▼
HTTP 204 No Content
```

### Código Real

```csharp
// PlaceService.cs
public ResultViewModel Book(
    int id,
    CreateBookInputModel model)
{
    // Validação
    bool exists = _context.Places.Any(p => p.Id == id);

    if (!exists)
    {
        return ResultViewModel.Error("Local não encontrado");
    }

    // Criar reserva
    PlaceBook book = new PlaceBook(
        model.IdUser,
        model.IdPlace,
        model.StartDate,
        model.EndDate,
        model.Comments
    );

    // Salvar
    _context.PlaceBooks.Add(book);
    _context.SaveChanges();

    // Sucesso sem dados
    return ResultViewModel.Success();
}
```

```csharp
// PlacesController.cs
[HttpPost("{id}/books")]
public IActionResult PostBook(
    int id,
    CreateBookInputModel model)
{
    _placeService.Book(id, model);

    return NoContent();  // 204 - Sem corpo
}
```

### Respostas

**Sucesso (204 No Content)**

```
Status: 204 No Content
Body: (vazio)
```

**Falha (200 OK com erro)**

```json
{
  "message": "Local não encontrado",
  "isSuccess": false
}
```

---

## 📊 Tabela Comparativa: Tipos de Respostas

| Caso              | Tipo de Return             | Factory Method         | Exemplo                                     | Status HTTP |
| ----------------- | -------------------------- | ---------------------- | ------------------------------------------- | ----------- |
| Sucesso com dados | `ResultViewModel<T>`       | `.Success(data)`       | `ResultViewModel<int>.Success(42)`          | 200/201     |
| Sucesso sem dados | `ResultViewModel`          | `.Success()`           | `ResultViewModel.Success()`                 | 204         |
| Erro com dados    | `ResultViewModel<T>`       | `.Error(default, msg)` | `ResultViewModel<T>.Error(null, "...")`     | 200         |
| Erro sem dados    | `ResultViewModel`          | `.Error(msg)`          | `ResultViewModel.Error("...")`              | 200         |
| Lista vazia OK    | `ResultViewModel<List<T>>` | `.Success([])`         | `ResultViewModel<List>.Success(new List())` | 200         |

---

## 🎯 Padrão de Implementação Passo a Passo

### Passo 1: Definir o modelo de entrada

```csharp
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
```

### Passo 2: Implementar no Service

```csharp
public ResultViewModel<int> CreatePlace(CreatePlaceInputModel model)
{
    // Validação 1: Verifica modelo
    if (string.IsNullOrEmpty(model.Title))
        return ResultViewModel<int>.Error(0, "Título obrigatório");

    // Validação 2: Verifica dados duplicados
    bool exists = _context.Places.Any(p => p.Title == model.Title);
    if (exists)
        return ResultViewModel<int>.Error(0, "Título já existe");

    // Lógica de negócio
    var place = new Place(model.Title, model.Description, ...);
    _context.Places.Add(place);
    _context.SaveChanges();

    // Retornar sucesso
    return ResultViewModel<int>.Success(place.Id);
}
```

### Passo 3: Usar no Controller

```csharp
[HttpPost]
public IActionResult Create(CreatePlaceInputModel model)
{
    var result = _placeService.CreatePlace(model);

    // Opção 1: Se tem sucesso, retornar 201
    if (result.IsSuccess)
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, model);

    // Opção 2: Se falha, retornar Ok com erro
    return Ok(result);
}
```

### Passo 4: Cliente recebe resposta padronizada

```json
{
  "data": 42,
  "message": "",
  "isSuccess": true
}
```

---

## 🔍 Debugging: Como Ler a Resposta

```
Recebeu:
{
  "data": null,
  "message": "Local não encontrado",
  "isSuccess": false
}

Interpretação:
✓ Operação falhou (isSuccess: false)
✓ Motivo: Local não encontrado (message)
✓ Sem dados retornados (data: null)
✓ Status HTTP: 200 OK (sucesso na chamada HTTP)

Próximo passo do cliente:
if (!result.isSuccess) {
  mostrarErro(result.message);  // "Local não encontrado"
}
```

---

## 🚀 Boas Práticas na Implementação

### ✅ Padrão Recomendado

```csharp
public ResultViewModel<User> GetUser(int id)
{
    // 1. Validar entrada
    if (id <= 0)
        return ResultViewModel<User>.Error(null, "ID inválido");

    // 2. Buscar no database
    var user = _context.Users.Find(id);

    // 3. Validar resultado
    if (user == null)
        return ResultViewModel<User>.Error(null, "Usuário não encontrado");

    // 4. Se tudo OK, retornar sucesso
    return ResultViewModel<User>.Success(user);
}
```

### ❌ Padrões a Evitar

```csharp
// ❌ NÃO: Retornar null
public User GetUser(int id)
{
    return _context.Users.Find(id);  // Ambíguo!
}

// ❌ NÃO: Lançar exceção para validação
public ResultViewModel<User> GetUser(int id)
{
    if (id <= 0)
        throw new ArgumentException();  // Misturar padrões!
    return ResultViewModel<User>.Success(...);
}

// ❌ NÃO: Mensagens de erro genéricas
return ResultViewModel.Error("Erro");  // De quê?!

// ❌ NÃO: Sempre retornar sucesso
public ResultViewModel<List<User>> GetAll()
{
    var users = _context.Users.ToList();
    return ResultViewModel<List<User>>.Success(users);  // E se vazio?
    // Na verdade, isso está OK! Lista vazia = sucesso
}
```

---

## 📝 Checklist de Implementação

Ao implementar uma nova funcionalidade, siga este checklist:

- [ ] **Service Layer**
  - [ ] Método retorna `ResultViewModel` ou `ResultViewModel<T>`
  - [ ] Validações retornam `.Error()`
  - [ ] Sucesso retorna `.Success()`
  - [ ] Mensagens de erro são claras

- [ ] **Controller Layer**
  - [ ] Recebe o `ResultViewModel`
  - [ ] Retorna `Ok()` para a resposta
  - [ ] Status HTTP apropriado (201 para criação, 204 para sem dados, etc)
  - [ ] Não revalida a lógica de negócio

- [ ] **Client Side**
  - [ ] Verifica `isSuccess` antes de usar `data`
  - [ ] Exibe `message` em caso de erro
  - [ ] Trata lista vazia como sucesso (não erro)

---

## 💬 Exemplos de Mensagens de Erro Recomendadas

```csharp
// ✅ Boas mensagens
"Usuário não encontrado"
"Email já cadastrado"
"Senha deve ter pelo menos 8 caracteres"
"Local não está disponível para este período"
"Você não tem permissão para editar este local"

// ❌ Ruins
"Erro"
"Falhou"
"Problema"
"Erro 404"
"Exception"
```

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0 - Exemplos Avançados  
**Status**: ✅ Completo
