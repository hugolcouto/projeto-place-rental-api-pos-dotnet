# 🛡️ Middleware & Exception Handling - Guia Completo

## Introdução

**Middleware** é código que roda em cada requisição HTTP. **Exception Handling Middleware** captura erros e retorna respostas padronizadas.

### 🎯 Objetivo

- ✅ Processar requisições/respostas
- ✅ Tratar exceções globalmente
- ✅ Retornar erros padronizados
- ✅ Registrar logs

---

## 🔄 Pipeline de Requisição

```
Cliente
  ↓
Requisição HTTP
  ↓
┌─────────────────────────────────────┐
│   MIDDLEWARE (Em ordem)             │
├─────────────────────────────────────┤
│ 1. Exception Handler ↓              │
│ 2. Authentication   ↓              │
│ 3. Authorization    ↓              │
│ 4. Logging          ↓              │
│ 5. CORS             ↓              │
│ ... mais middlewares ...            │
│                     ↓              │
│ Controller/Action   ↓              │
│                     ↓              │
└─────────────────────────────────────┘
  ↓
Resposta HTTP
  ↓
Cliente
```

---

## 🛡️ Exception Handler no PlaceRental

### Implementação

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
            // ✅ Tratar NotFoundException especificamente
            details = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found"
            };
        }
        else
        {
            // ✅ Tratar outros erros como 500
            details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error"
            };
        }

        // ✅ Setar status code
        httpContext.Response.StatusCode =
            details.Status ?? StatusCodes.Status500InternalServerError;

        // ✅ Retornar JSON
        await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);

        return true; // ✅ Indicar que tratou
    }
}
```

---

### Registrar no Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// ✅ Registrar exception handler
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ✅ Usar exception handler no pipeline
app.UseExceptionHandler();

app.MapControllers();
app.Run();
```

---

## 🌊 Fluxo de Erro

```
Controller
  ↓
Lança NotFoundException("Usuário não encontrado")
  ↓
Não é capturada no controller
  ↓
Sobe para o middleware
  ↓
ApiExceptionHandler.TryHandleAsync()
  ↓
Verifica tipo (NotFoundException)
  ↓
Cria ProblemDetails com status 404
  ↓
Retorna JSON ao cliente
  ↓
HTTP 404 + JSON
```

---

## 📋 Tipos de Erros

### 1. NotFoundException (404)

```csharp
// Lançar no service
public ResultViewModel<User> GetById(int id)
{
    var user = _context.Users.Find(id);
    if (user == null)
        throw new NotFoundException("Usuário não encontrado");

    return ResultViewModel<User>.Success(user);
}

// Handler captura
if (exception is NotFoundException)
{
    details = new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Not Found"
    };
}

// JSON retornado
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Not Found",
    "status": 404
}
```

---

### 2. Erro 500 (Genérico)

```csharp
// Qualquer outra exceção
public ResultViewModel GetData()
{
    int result = 10 / int.Parse("0"); // ❌ DivideByZeroException
}

// Handler não reconhece tipo específico
else
{
    details = new ProblemDetails
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "Server Error"
    };
}

// JSON retornado
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "Server Error",
    "status": 500
}
```

---

## 🎯 Estender o Handler

```csharp
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // ✅ Logar exceção
        _logger.LogError(exception, "Ocorreu um erro");

        ProblemDetails details;

        if (exception is NotFoundException notFound)
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = notFound.Message
            };
        }
        else if (exception is ArgumentException argEx)
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = argEx.Message
            };
        }
        else if (exception is UnauthorizedAccessException)
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized"
            };
        }
        else
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = "Um erro interno ocorreu"
            };
        }

        httpContext.Response.StatusCode =
            details.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);

        return true;
    }
}
```

---

## 📝 Criar Middleware Customizado

```csharp
// 1. Criar classe
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ✅ Antes da requisição
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

        // ✅ Passar para próximo middleware
        await _next(context);

        // ✅ Depois da resposta
        _logger.LogInformation($"Response: {context.Response.StatusCode}");
    }
}

// 2. Registrar no Program.cs
app.UseMiddleware<LoggingMiddleware>();

// 3. Middleware será executado em cada requisição
```

---

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Usar AddExceptionHandler para registrar
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ✅ Usar app.UseExceptionHandler() no pipeline
app.UseExceptionHandler();

// ✅ Retornar ProblemDetails padrão
details = new ProblemDetails { ... };

// ✅ Logar exceções
_logger.LogError(exception, "Erro ocorreu");

// ✅ Ordem correta no pipeline
app.UseExceptionHandler();    // Primeiro!
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### ❌ DON'T

```csharp
// ❌ Não registrar handler
// builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ❌ Não usar no pipeline
// app.UseExceptionHandler();

// ❌ Retornar resposta customizada sem padrão
// return new { message = "Erro" };

// ❌ Não logar
// Silenciar exceções!

// ❌ Ordem errada no pipeline
app.MapControllers();        // Errado! Deve ser depois do handler
app.UseExceptionHandler();   // Muito tarde!
```

---

## 🎓 Conclusão

Middleware é essencial para:

- 🛡️ Tratamento centralizado de erros
- 📝 Logging e auditoria
- 📊 Padronização de respostas
- 🔒 Segurança

**No PlaceRental**, `ApiExceptionHandler` trata todas as exceções globalmente!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
