# 📋 Padrão Result - Guia Completo

## Introdução

O **Padrão Result** é uma abordagem elegante e padronizada para tratar operações que podem ter sucesso ou falha em sua aplicação. Em vez de usar exceções para tudo ou apenas retornar valores nulos, o padrão Result encapsula tanto o sucesso quanto a falha em um objeto único e estruturado.

### 🎯 Por que usar o Padrão Result?

- ✅ **Uniformidade**: Todas as operações retornam o mesmo formato
- ✅ **Segurança**: Força o desenvolvedor a tratar possíveis falhas
- ✅ **Clareza**: O consumidor da API sabe exatamente o que esperar
- ✅ **Flexibilidade**: Pode retornar dados ou mensagens de erro
- ✅ **Rastreabilidade**: Fácil de registrar em logs e monitorar

---

## 🏗️ Estrutura do ResultViewModel

Seu projeto implementa duas versões do `ResultViewModel`:

### 1️⃣ ResultViewModel (Não Genérico)

Usado quando você **não precisa retornar dados específicos**, apenas informações sobre sucesso ou falha.

```csharp
public class ResultViewModel
{
    public ResultViewModel(string message = "", bool isSuccess = true)
    {
        Message = message;
        IsSuccess = isSuccess;
    }

    public string Message { get; set; }
    public bool IsSuccess { get; set; }

    // Factory Methods
    public static ResultViewModel Success() => new();
    public static ResultViewModel Error(string message) => new(message, false);
}
```

**Estrutura:**

- **Message**: Mensagem descritiva (vazia em caso de sucesso, erro em caso de falha)
- **IsSuccess**: Booleano indicando sucesso (true) ou falha (false)

**Exemplo de uso:**

```csharp
// Sucesso
ResultViewModel result = ResultViewModel.Success();
// Output: { "message": "", "isSuccess": true }

// Erro
ResultViewModel result = ResultViewModel.Error("O local não foi encontrado");
// Output: { "message": "O local não foi encontrado", "isSuccess": false }
```

### 2️⃣ ResultViewModel\<T\> (Genérico)

Estende o `ResultViewModel` não-genérico e adiciona a capacidade de retornar **dados tipados**.

```csharp
public class ResultViewModel<T> : ResultViewModel
{
    public ResultViewModel(T? data, string message = "", bool isSuccess = true)
        : base() => Data = data;

    public T? Data { get; set; }

    // Factory Methods
    public static ResultViewModel<T> Success(T data) => new(data);
    public static ResultViewModel<T> Error(T data, string message) => new(default, message, false);
}
```

**Estrutura:**

- **Data**: Dados genéricos do tipo `T`
- **Message** e **IsSuccess**: Herdados de `ResultViewModel`

**Exemplo de uso:**

```csharp
// Sucesso retornando um objeto
var place = new Place { Id = 1, Title = "Casa na praia" };
ResultViewModel<Place> result = ResultViewModel<Place>.Success(place);
// Output: {
//   "data": { "id": 1, "title": "Casa na praia", ... },
//   "message": "",
//   "isSuccess": true
// }

// Erro
ResultViewModel<Place> result = ResultViewModel<Place>.Error(null, "Local não encontrado");
// Output: {
//   "data": null,
//   "message": "Local não encontrado",
//   "isSuccess": false
// }
```

---

## 🔄 Fluxo de Dados - Visão Geral

```
┌──────────────┐
│   Cliente    │ (Frontend / Postman / etc)
└──────┬───────┘
       │ HTTP Request
       │
       ▼
┌──────────────────────┐
│   API Controller     │ PlacesController
└──────┬───────────────┘
       │ Chama
       ▼
┌──────────────────────┐
│   Service Layer      │ PlaceService
│  (Business Logic)    │
└──────┬───────────────┘
       │ Retorna
       ▼
┌──────────────────────┐
│  ResultViewModel     │ Resultado da operação
│  ou                  │
│  ResultViewModel<T>  │
└──────┬───────────────┘
       │ JSON Response
       ▼
┌──────────────────────┐
│   Cliente (JSON)     │
└──────────────────────┘
```

---

## 💡 Exemplos Práticos do Projeto

### Exemplo 1: Obter Local por ID (Com Dados)

**Operação**: Buscar um local específico e retornar seus detalhes

#### 🔵 No Service (PlaceService.cs)

```csharp
public ResultViewModel<PlaceDetailsViewModel?> GetById(int id)
{
    Place? place = _context.Places.SingleOrDefault(p => p.Id == id);

    // Se o lugar foi encontrado
    if (place != null)
    {
        var viewModel = PlaceDetailsViewModel.FromEntity(place);
        return ResultViewModel<PlaceDetailsViewModel>.Success(viewModel);
    }

    // Se não encontrou
    return ResultViewModel<PlaceDetailsViewModel>.Error(null, "Local não encontrado");
}
```

#### 🔴 No Controller (PlacesController.cs)

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    ResultViewModel<PlaceDetailsViewModel> place = _placeService.GetById(id)!;

    return Ok(place);
}
```

#### 📤 Resposta HTTP

**Sucesso (200 OK):**

```json
{
  "data": {
    "id": 1,
    "title": "Casa na praia",
    "description": "Linda casa à beira-mar",
    "dailyPrice": 150.0,
    "amenities": [
      { "id": 1, "description": "WiFi" },
      { "id": 2, "description": "Piscina" }
    ]
  },
  "message": "",
  "isSuccess": true
}
```

**Falha (200 OK com flag de erro):**

```json
{
  "data": null,
  "message": "Local não encontrado",
  "isSuccess": false
}
```

---

### Exemplo 2: Criar Local (Retornando Apenas ID)

**Operação**: Criar um novo local e retornar seu ID

#### 🔵 No Service

```csharp
public ResultViewModel<int> Inset(CreatePlaceInputModel model)
{
    var address = new Address(
        model.Address.Street,
        model.Address.Number,
        // ... outros campos
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

    _context.Places.Add(place);
    _context.SaveChanges();

    // Retorna o ID do local criado
    return ResultViewModel<int>.Success(place.Id);
}
```

#### 🔴 No Controller

```csharp
[HttpPost]
public IActionResult Post(CreatePlaceInputModel model)
{
    var PlaceId = _placeService.Inset(model);

    return CreatedAtAction(
        nameof(GetById),
        new { id = PlaceId.Data },  // Usa o ID do ResultViewModel
        model
    );
}
```

#### 📤 Resposta HTTP

**Sucesso (201 Created):**

```json
{
  "data": 42,
  "message": "",
  "isSuccess": true
}
```

**Locação**: `Location: /api/places/42`

---

### Exemplo 3: Reservar Local (Sem Dados)

**Operação**: Criar uma reserva para um local (sem necessidade de retornar dados)

#### 🔵 No Service

```csharp
public ResultViewModel Book(int id, CreateBookInputModel model)
{
    bool exists = _context.Places.Any(p => p.Id == id);

    if (!exists)
    {
        return ResultViewModel.Error("Local não encontrado");
    }

    PlaceBook book = new PlaceBook(
        model.IdUser,
        model.IdPlace,
        model.StartDate,
        model.EndDate,
        model.Comments
    );

    _context.PlaceBooks.Add(book);
    _context.SaveChanges();

    return ResultViewModel.Success();
}
```

#### 🔴 No Controller

```csharp
[HttpPost("{id}/books")]
public IActionResult PostBook(int id, CreateBookInputModel model)
{
    _placeService.Book(id, model);

    return NoContent();  // Retorna 204
}
```

#### 📤 Resposta HTTP

**Sucesso (204 No Content):**

```
(Sem corpo)
```

**Falha (200 OK com erro):**

```json
{
  "message": "Local não encontrado",
  "isSuccess": false
}
```

---

## 🛡️ Tratamento de Exceções

Seu projeto utiliza um middleware especial para tratar exceções de forma elegante:

### ApiExceptionHandler

```csharp
public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? details;

        if (exception is NotFoundException)
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found"
            };
        }
        else
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error"
            };
        }

        httpContext.Response.StatusCode = details.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);

        return true;
    }
}
```

### Como Funciona

1. **Erro capturado** durante a execução do endpoint
2. **Middleware intercepta** a exceção
3. **Verifica o tipo** da exceção
4. **Mapeia para ProblemDetails** com status HTTP apropriado
5. **Retorna resposta padronizada** em JSON

---

## 🎭 Padrões de Uso Recomendados

### ✅ Padrão 1: Sucesso com Dados

```csharp
// Service
public ResultViewModel<User> GetUser(int id)
{
    var user = _context.Users.Find(id);
    if (user == null)
        return ResultViewModel<User>.Error(null, "Usuário não encontrado");

    return ResultViewModel<User>.Success(user);
}

// Controller
[HttpGet("{id}")]
public IActionResult GetUser(int id)
{
    var result = _userService.GetUser(id);
    return Ok(result);
}
```

### ✅ Padrão 2: Sucesso sem Dados

```csharp
// Service
public ResultViewModel Delete(int id)
{
    var place = _context.Places.Find(id);
    if (place == null)
        return ResultViewModel.Error("Local não encontrado");

    place.IsDeleted = true;
    _context.SaveChanges();
    return ResultViewModel.Success();
}

// Controller
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    var result = _placeService.Delete(id);
    return NoContent();
}
```

### ✅ Padrão 3: Retornar Lista

```csharp
// Service
public ResultViewModel<List<PlaceViewModel>> GetAll(string search)
{
    var places = _context.Places
        .Where(p => p.Title.Contains(search))
        .Select(PlaceViewModel.FromEntity)
        .ToList();

    return ResultViewModel<List<PlaceViewModel>>.Success(places);
}

// Controller
[HttpGet]
public IActionResult GetAll(string search = "")
{
    var result = _placeService.GetAll(search);
    return Ok(result);
}
```

---

## 📊 Fluxograma Detalhado: Ciclo Completo

```
                        ┌─ REQUEST HTTP ─┐
                        │  GET /api/1    │
                        └────────┬────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  PlacesController       │
                    │  GetById(int id)        │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  PlaceService           │
                    │  GetById(int id)        │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  Consultar Database     │
                    │  SingleOrDefault(id)    │
                    └────────────┬────────────┘
                                 │
                ┌────────────────┼────────────────┐
                │                                 │
        ┌───────▼──────┐              ┌──────────▼────────┐
        │ Place Existe │              │ Place Não Existe  │
        └───────┬──────┘              └──────────┬────────┘
                │                                │
     ┌──────────▼──────────┐         ┌──────────▼──────────┐
     │ ResultViewModel<T>  │         │ ResultViewModel<T>  │
     │ .Success(place)     │         │ .Error(null,msg)    │
     └──────────┬──────────┘         └──────────┬──────────┘
                │                               │
                │       ┌───────────────────────┘
                │       │
                └───────┼──────────────┐
                        │              │
                ┌───────▼──────┐       │
                │   Ok(result) │       │
                └───────┬──────┘       │
                        │              │
        ┌───────────────┴─────────────▼──────┐
        │      HTTP Response (200 OK)         │
        │                                     │
        │  {                                  │
        │    "data": {...},                   │
        │    "message": "",                   │
        │    "isSuccess": true                │
        │  }                                  │
        │                                     │
        │  OU                                 │
        │                                     │
        │  {                                  │
        │    "data": null,                    │
        │    "message": "Não encontrado",    │
        │    "isSuccess": false               │
        │  }                                  │
        └─────────────────────────────────────┘
```

---

## 🔗 Fluxograma: Requisição POST (Criar)

```
                    ┌──────────────────┐
                    │ REQUEST HTTP POST│
                    │ /api/places      │
                    │ Body: {...}      │
                    └────────┬─────────┘
                             │
                ┌────────────▼────────────┐
                │  PlacesController       │
                │  Post(model)            │
                └────────────┬────────────┘
                             │
                ┌────────────▼────────────┐
                │  PlaceService.Insert()  │
                │  - Criar entidade       │
                │  - Adicionar no Context │
                │  - Salvar DB            │
                └────────────┬────────────┘
                             │
                ┌────────────▼──────────────────┐
                │ ResultViewModel<int>          │
                │ .Success(place.Id)            │
                │ Retorna: { data: 42, ... }   │
                └────────────┬──────────────────┘
                             │
                ┌────────────▼────────────┐
                │ CreatedAtAction(...)    │
                │ Status: 201             │
                │ Location: /api/places/42│
                └────────────┬────────────┘
                             │
        ┌────────────────────▼─────────────────────┐
        │      HTTP Response (201 Created)         │
        │      Location: /api/places/42            │
        │      Body: { data: 42, ... }             │
        └──────────────────────────────────────────┘
```

---

## 📈 Comparação: Com vs Sem Padrão Result

### ❌ Sem Padrão Result (Abordagem Tradicional)

```csharp
// Service - Sem padronização
public Place GetPlace(int id)
{
    return _context.Places.SingleOrDefault(p => p.Id == id);
    // Retorna null se não encontrar - cliente não sabe se é erro!
}

// Controller - Precisa verificar null manualmente
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var place = _placeService.GetPlace(id);

    if (place == null)
        return NotFound();

    return Ok(place);
}
```

**Problemas:**

- Inconsistência: Cada endpoint pode fazer de uma forma diferente
- Ambiguidade: null pode significar várias coisas
- Sem mensagem de erro: Cliente não sabe por que falhou
- Sem flag de sucesso: Cliente precisa adivinhar o estado

---

### ✅ Com Padrão Result (Abordagem Padronizada)

```csharp
// Service - Sempre retorna ResultViewModel
public ResultViewModel<Place> GetPlace(int id)
{
    var place = _context.Places.SingleOrDefault(p => p.Id == id);

    if (place == null)
        return ResultViewModel<Place>.Error(null, "Local não encontrado");

    return ResultViewModel<Place>.Success(place);
}

// Controller - Simples e direto
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var result = _placeService.GetPlace(id);
    return Ok(result);
}
```

**Vantagens:**

- Consistência: Todos os endpoints seguem o mesmo padrão
- Clareza: Sempre tem message e isSuccess
- Segurança: Força tratar ambos os casos
- Rastreabilidade: Fácil logar ou monitorar

---

## 🎓 Resumo Visual

```
┌──────────────────────────────────────────────────────┐
│              PADRÃO RESULT                           │
├──────────────────────────────────────────────────────┤
│                                                      │
│  ResultViewModel (Não Genérico)                      │
│  ├─ Message: string (erro ou vazio)                 │
│  ├─ IsSuccess: bool                                 │
│  └─ Factory Methods: Success(), Error(msg)         │
│                                                      │
│  ResultViewModel<T> (Genérico)                       │
│  ├─ Data: T? (dados retornados)                     │
│  ├─ Message: string (herdado)                       │
│  ├─ IsSuccess: bool (herdado)                       │
│  └─ Factory Methods: Success(data), Error(data,msg)│
│                                                      │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│              COMO USAR                               │
├──────────────────────────────────────────────────────┤
│                                                      │
│  1. Service retorna ResultViewModel ou              │
│     ResultViewModel<T>                              │
│                                                      │
│  2. Controller recebe o resultado                   │
│                                                      │
│  3. Controller retorna Ok(resultado) para JSON      │
│                                                      │
│  4. Cliente recebe resposta padronizada             │
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

## 🚀 Boas Práticas

### ✅ DO

```csharp
// ✅ Sempre retornar ResultViewModel
public ResultViewModel<User> CreateUser(CreateUserModel model)
{
    // validar
    if (string.IsNullOrEmpty(model.Name))
        return ResultViewModel<User>.Error(null, "Nome é obrigatório");

    // criar
    var user = new User { Name = model.Name };
    _context.Users.Add(user);
    _context.SaveChanges();

    return ResultViewModel<User>.Success(user);
}

// ✅ Usar factory methods
var success = ResultViewModel<int>.Success(42);
var error = ResultViewModel<int>.Error(0, "Erro!");

// ✅ Ter mensagens claras
return ResultViewModel.Error("Usuário com email já existe");
```

### ❌ DON'T

```csharp
// ❌ Retornar null
public User GetUser(int id)
{
    return _context.Users.Find(id); // pode ser null!
}

// ❌ Misturar padrões
public void CreatePlace(CreatePlaceModel model) // void!
{
    // ...
}

// ❌ Mensagens vagas
return ResultViewModel.Error("Erro"); // De quê?!

// ❌ Não verificar isSuccess
var result = _service.GetUser(1);
var user = result.Data; // Pode ser null!
```

---

## 📚 Referências

- **Repository Pattern**: A lógica de acesso a dados está encapsulada
- **Service Pattern**: A lógica de negócio fica no serviço
- **Factory Pattern**: `Success()` e `Error()` são factory methods
- **Railway-Oriented Programming**: Padrão que inspira o Result pattern

---

## 🎯 Conclusão

O Padrão Result é uma forma elegante e padronizada de:

- ✅ Retornar sucesso OU falha
- ✅ Incluir dados ou mensagens de erro
- ✅ Manter consistência em toda a API
- ✅ Facilitar o tratamento de erros no cliente
- ✅ Melhorar a rastreabilidade e manutenibilidade

No contexto do **PlaceRental**, o padrão garante que todo serviço (criar local, buscar reservas, etc) retorna no mesmo formato, facilitando tanto o desenvolvimento quanto o consumo da API.

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
