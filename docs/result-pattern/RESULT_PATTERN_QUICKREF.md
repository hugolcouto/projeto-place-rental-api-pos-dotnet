# 🚀 Result Pattern - Guia de Referência Rápida

> Bookmark esta página! Use como referência rápida enquanto codifica.

---

## ⚡ Quick Start - Copy & Paste

### Template 1: Service com Sucesso/Erro

```csharp
public ResultViewModel<T> MinhaOperacao(int id)
{
    // Validação
    var item = _context.Items.Find(id);
    if (item == null)
        return ResultViewModel<T>.Error(null, "Item não encontrado");

    // Lógica
    // ... fazer algo ...

    // Retorno
    return ResultViewModel<T>.Success(item);
}
```

### Template 2: Controller

```csharp
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var result = _service.MinhaOperacao(id);
    return Ok(result);
}
```

### Template 3: Service Sem Dados

```csharp
public ResultViewModel MinhaOperacao(int id)
{
    var item = _context.Items.Find(id);
    if (item == null)
        return ResultViewModel.Error("Item não encontrado");

    // ... fazer algo ...
    return ResultViewModel.Success();
}
```

---

## 📚 Cheat Sheet - Métodos Disponíveis

### ResultViewModel (Não-Genérico)

```csharp
// Criar sucesso
ResultViewModel.Success()
// Output: { message: "", isSuccess: true }

// Criar erro
ResultViewModel.Error("Descrição do erro")
// Output: { message: "Descrição do erro", isSuccess: false }

// Acessar propriedades
result.IsSuccess  // bool
result.Message    // string
```

### ResultViewModel<T> (Genérico)

```csharp
// Criar sucesso com dados
ResultViewModel<int>.Success(42)
// Output: { data: 42, message: "", isSuccess: true }

// Criar erro sem dados
ResultViewModel<int>.Error(null, "Erro aqui")
// Output: { data: null, message: "Erro aqui", isSuccess: false }

// Acessar propriedades
result.Data       // T? (pode ser null)
result.IsSuccess  // bool
result.Message    // string
```

---

## 🎯 Quando Usar Cada Um

### Use `ResultViewModel`

- ✅ Delete (sem dados de retorno)
- ✅ Update (sem dados de retorno)
- ✅ Validação de regra de negócio
- ✅ Operações que não retornam valor

```csharp
public ResultViewModel Delete(int id)
{
    var item = _context.Items.Find(id);
    if (item == null)
        return ResultViewModel.Error("Não encontrado");

    _context.Items.Remove(item);
    _context.SaveChanges();

    return ResultViewModel.Success();
}
```

### Use `ResultViewModel<T>`

- ✅ Get (retorna objeto)
- ✅ Create (retorna ID ou objeto)
- ✅ Update (retorna objeto atualizado)
- ✅ List (retorna `List<T>`)

```csharp
public ResultViewModel<User> GetUser(int id)
{
    var user = _context.Users.Find(id);
    if (user == null)
        return ResultViewModel<User>.Error(null, "Não encontrado");

    return ResultViewModel<User>.Success(user);
}
```

---

## 🔄 Fluxo Padrão (3 Linhas)

```csharp
// 1. Validar
if (condicao_invalida) return ResultViewModel.Error("mensagem");

// 2. Executar
var resultado = /* operação */;

// 3. Retornar
return ResultViewModel.Success(resultado);
```

---

## 🌊 Fluxo Visual (ASCII Art)

### GET com Sucesso

```
┌─────────────┐
│ GET /id     │
└────────┬────┘
         ▼
   ┌──────────┐
   │ Buscar   │ ✓ Encontrou
   └────┬─────┘
        ▼
   ┌─────────────────────────┐
   │ Success(objeto)         │
   │ { data: {...} }         │
   └─────────────────────────┘
```

### GET com Falha

```
┌─────────────┐
│ GET /id     │
└────────┬────┘
         ▼
   ┌──────────┐
   │ Buscar   │ ✗ Não encontrou
   └────┬─────┘
        ▼
   ┌─────────────────────────┐
   │ Error(null, "msg")      │
   │ { data: null }          │
   └─────────────────────────┘
```

### POST (Criação)

```
┌──────────────────┐
│ POST com corpo   │
└────────┬─────────┘
         ▼
   ┌──────────────┐
   │ Criar objeto │
   └────────┬─────┘
         ▼
   ┌──────────────────────────┐
   │ Success(novoId)          │
   │ { data: 42 }             │
   │ Status: 201 Created      │
   └──────────────────────────┘
```

---

## 📦 Estrutura da Resposta JSON

### Sucesso

```json
{
  "data": <valor_do_tipo_T>,
  "message": "",
  "isSuccess": true
}
```

### Erro

```json
{
  "data": null,
  "message": "Descrição clara do erro",
  "isSuccess": false
}
```

### Lista (mesmo com vazia)

```json
{
  "data": [],
  "message": "",
  "isSuccess": true
}
```

---

## 🎓 Exemplo Completo (Copy Ready)

### Service

```csharp
using PlaceRentalApp.Application.Models;
using PlaceRentalApp.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PlaceRentalApp.Application.Services;

public class UserService : IUserService
{
    private readonly PlaceRentalDbContext _context;

    public UserService(PlaceRentalDbContext context)
    {
        _context = context;
    }

    // GET: Retorna usuário ou erro
    public ResultViewModel<User> GetById(int id)
    {
        if (id <= 0)
            return ResultViewModel<User>.Error(null, "ID inválido");

        var user = _context.Users.Find(id);

        if (user == null)
            return ResultViewModel<User>.Error(null, "Usuário não encontrado");

        return ResultViewModel<User>.Success(user);
    }

    // GET: Retorna lista
    public ResultViewModel<List<User>> GetAll()
    {
        var users = _context.Users.ToList();
        return ResultViewModel<List<User>>.Success(users);
    }

    // POST: Cria e retorna ID
    public ResultViewModel<int> Create(CreateUserInputModel model)
    {
        if (string.IsNullOrEmpty(model.Name))
            return ResultViewModel<int>.Error(0, "Nome obrigatório");

        var user = new User(model.Name, model.Email);
        _context.Users.Add(user);
        _context.SaveChanges();

        return ResultViewModel<int>.Success(user.Id);
    }

    // DELETE: Sem retorno de dados
    public ResultViewModel Delete(int id)
    {
        var user = _context.Users.Find(id);

        if (user == null)
            return ResultViewModel.Error("Usuário não encontrado");

        _context.Users.Remove(user);
        _context.SaveChanges();

        return ResultViewModel.Success();
    }
}
```

### Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using PlaceRentalApp.Application.Services;
using PlaceRentalApp.Application.Models;

namespace PlaceRentalApp.API.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _userService.GetById(id);
        return Ok(result);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _userService.GetAll();
        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create(CreateUserInputModel model)
    {
        var result = _userService.Create(model);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { id = result.Data }, model);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _userService.Delete(id);
        return NoContent();
    }
}
```

---

## 🔍 Debugging - O que Cada Resposta Significa

| Resposta                           | Interpretação          | Ação                   |
| ---------------------------------- | ---------------------- | ---------------------- |
| `{ data: {...}, isSuccess: true }` | ✅ Sucesso completo    | Usar `data`            |
| `{ data: null, isSuccess: false }` | ❌ Erro                | Exibir `message`       |
| `{ data: [], isSuccess: true }`    | ✅ Sem resultados (OK) | Não é erro!            |
| `{ data: 42, isSuccess: true }`    | ✅ Sucesso com ID      | Usar para redirecionar |
| `{ message: "", isSuccess: true }` | ✅ Sucesso sem dados   | OK, operação feita     |

---

## 🚨 Erros Comuns e Soluções

### ❌ Erro 1: Retornar null diretamente

```csharp
// ❌ ERRADO
public User GetUser(int id) => _context.Users.Find(id); // pode ser null!

// ✅ CORRETO
public ResultViewModel<User> GetUser(int id)
{
    var user = _context.Users.Find(id);
    if (user == null)
        return ResultViewModel<User>.Error(null, "Não encontrado");
    return ResultViewModel<User>.Success(user);
}
```

### ❌ Erro 2: Esquecer de verificar IsSuccess

```csharp
// ❌ ERRADO
var result = _service.GetUser(1);
var name = result.Data.Name;  // Pode ser null!

// ✅ CORRETO
var result = _service.GetUser(1);
if (result.IsSuccess)
{
    var name = result.Data.Name;
}
else
{
    // Tratar erro
}
```

### ❌ Erro 3: Mensagens de erro vagas

```csharp
// ❌ ERRADO
return ResultViewModel.Error("Erro");
return ResultViewModel.Error("Falhou");

// ✅ CORRETO
return ResultViewModel.Error("Usuário não encontrado");
return ResultViewModel.Error("Email já está cadastrado");
```

### ❌ Erro 4: Lançar exceção em service

```csharp
// ❌ ERRADO (mistura padrões)
public ResultViewModel<User> GetUser(int id)
{
    if (id <= 0)
        throw new ArgumentException();

    var user = _context.Users.Find(id);
    if (user == null)
        throw new NotFoundException();

    return ResultViewModel<User>.Success(user);
}

// ✅ CORRETO (consistente)
public ResultViewModel<User> GetUser(int id)
{
    if (id <= 0)
        return ResultViewModel<User>.Error(null, "ID inválido");

    var user = _context.Users.Find(id);
    if (user == null)
        return ResultViewModel<User>.Error(null, "Não encontrado");

    return ResultViewModel<User>.Success(user);
}
```

---

## 🎯 Status HTTP Recomendados

| Operação | Sucesso        | Erro                       | Exemplo                                |
| -------- | -------------- | -------------------------- | -------------------------------------- |
| GET      | 200 OK         | 200 com `isSuccess: false` | `{ isSuccess: false, message: "..." }` |
| POST     | 201 Created    | 200 com `isSuccess: false` | `{ isSuccess: false, message: "..." }` |
| PUT      | 200 OK         | 200 com `isSuccess: false` | `{ isSuccess: false, message: "..." }` |
| DELETE   | 204 No Content | 200 com `isSuccess: false` | `{ isSuccess: false, message: "..." }` |
| PATCH    | 200 OK         | 200 com `isSuccess: false` | `{ isSuccess: false, message: "..." }` |

> 💡 **Nota**: Mesmo em erros, retorna 200 OK. O `isSuccess: false` indica o erro!

---

## 🧪 Testando com Postman

### GET - Buscar Usuário

```
GET http://localhost:5000/api/users/1

Response:
{
  "data": {
    "id": 1,
    "name": "João",
    "email": "joao@email.com"
  },
  "message": "",
  "isSuccess": true
}
```

### GET - Não Encontrado

```
GET http://localhost:5000/api/users/999

Response:
{
  "data": null,
  "message": "Usuário não encontrado",
  "isSuccess": false
}
```

### POST - Criar

```
POST http://localhost:5000/api/users
Content-Type: application/json

{
  "name": "Maria",
  "email": "maria@email.com"
}

Response (201 Created):
Location: http://localhost:5000/api/users/2
{
  "data": 2,
  "message": "",
  "isSuccess": true
}
```

---

## 💾 Salvar como Snippet (VS Code)

Adicione ao seu `snippets.json` para auto-complete:

```json
{
  "ResultViewModel Success": {
    "prefix": "resultSuccess",
    "body": "ResultViewModel<$1>.Success($2)",
    "description": "Result ViewModel Success"
  },
  "ResultViewModel Error": {
    "prefix": "resultError",
    "body": "ResultViewModel<$1>.Error(null, \"$2\")",
    "description": "Result ViewModel Error"
  },
  "Service Method Template": {
    "prefix": "serviceMethod",
    "body": [
      "public ResultViewModel<$1> $2(int id)",
      "{",
      "    var item = _context.$3.Find(id);",
      "    if (item == null)",
      "        return ResultViewModel<$1>.Error(null, \"Não encontrado\");",
      "    ",
      "    return ResultViewModel<$1>.Success(item);",
      "}"
    ],
    "description": "Service method with Result pattern"
  }
}
```

---

## 📱 ClientSide (JavaScript/Frontend)

### React Hook

```javascript
const { data, isSuccess, message } = await fetch("/api/users/1").then((r) =>
  r.json(),
);

if (isSuccess) {
  console.log("Usuário:", data);
} else {
  console.error("Erro:", message);
}
```

### Axios Interceptor

```javascript
api.interceptors.response.use((response) => {
  const { isSuccess, message, data } = response.data;

  if (!isSuccess) {
    toast.error(message);
  }

  return data;
});
```

---

## ✅ Checklist Final

Antes de fazer um commit, verifique:

- [ ] Service retorna `ResultViewModel` ou `ResultViewModel<T>`
- [ ] Todas as validações usam `.Error()`
- [ ] Sucesso usa `.Success()`
- [ ] Mensagens de erro são descritivas
- [ ] Controller chama o service e retorna `Ok(result)`
- [ ] Testes verificam `isSuccess` e não apenas `data`
- [ ] Documentação menciona o padrão

---

## 🔗 Links Relacionados

- 📖 [Guia Completo](./RESULT_PATTERN.md)
- 🔥 [Exemplos Avançados](./RESULT_PATTERN_ADVANCED.md)
- 💻 [Código do Projeto](../PlaceRentalApp.Application/Models/ResultViewModel.cs)

---

**Última atualização**: 16 de Abril de 2026  
**Versão**: 1.0 - Quick Reference  
**Mantido por**: Tim do Projeto

> 💡 Dica: Bookmark esta página no seu navegador para fácil acesso!
