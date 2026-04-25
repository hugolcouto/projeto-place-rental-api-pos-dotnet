# JWT no PlaceRentalApp

Este guia mostra como o JWT esta implementado no projeto e como o Swagger foi configurado para testar a autenticacao no navegador.

## Visao geral do fluxo

O fluxo do projeto segue esta ordem:

1. O usuario envia email e senha.
2. A senha e convertida para hash no `AuthService`.
3. O `UserService` valida as credenciais no repositorio.
4. Se o usuario existir, o `AuthService` gera um token JWT.
5. O token e enviado para o cliente no `LoginViewModel`.
6. O cliente usa o token no header `Authorization: Bearer <token>`.
7. O pipeline da API valida o token com `JwtBearer`.
8. Os endpoints protegidos podem ser acessados com `[Authorize]`.

## Onde isso fica no projeto

- Configuracao de autenticacao: `PlaceRentalApp.Infrastructure/InfrastructureModule.cs`
- Geracao do token: `PlaceRentalApp.Infrastructure/Auth/AuthService.cs`
- Validacao do segredo JWT: `PlaceRentalApp.Infrastructure/Auth/JwtKeyHelper.cs`
- Regras de login: `PlaceRentalApp.Application/Services/User/UserService.cs`
- Modelos de entrada e saida: `PlaceRentalApp.Application/Models/LoginInputModel.cs`
- Configuracao da API e Swagger: `PlaceRentalApp.API/Program.cs`
- Configuracao de ambiente: `PlaceRentalApp.API/appsettings.json`

## 1. Configuracao em appsettings.json

O JWT usa tres valores basicos:

```json
"JWT": {
  "Key": "<JWT Key>",
  "Issuer": "Place Rental App",
  "Audience": "PlaceRentalApp"
}
```

Significado de cada item:

- `Key`: chave secreta usada para assinar e validar o token.
- `Issuer`: emissor do token.
- `Audience`: publico esperado do token.

Esses valores precisam ser iguais na geracao e na validacao.

### Tamanho minimo da chave

O algoritmo usado neste projeto e `HS256`, que exige uma chave com pelo menos 16 bytes, ou 128 bits.

Se o segredo for menor do que isso, a aplicacao vai falhar quando tentar criar o token ou validar a assinatura.

Importante: a validacao e feita em bytes UTF-8 (via `Encoding.UTF8.GetBytes(key)`), nao apenas pela quantidade de caracteres.

### Exemplo seguro

Prefira usar uma chave longa e aleatoria, por exemplo:

```json
"JWT": {
    "Key": "2f8d7f6d2f0c4f8e91b7c1f0d7d9a9b1c8e4f2b7a6d1f0c9e8a7b6c5d4e3f2a1",
    "Issuer": "Place Rental App",
    "Audience": "PlaceRentalApp"
}
```

Em ambiente local, o segredo tambem pode ficar em user secrets ou variaveis de ambiente, desde que o valor final continue com tamanho suficiente.

### Onde definir a chave localmente

O `appsettings.json` da API ainda usa um placeholder, entao ele nao deve ser tratado como a fonte real da chave.

No desenvolvimento, prefira uma destas opcoes:

- `appsettings.Development.json` com um valor valido de `JWT:Key`
- user secrets
- variavel de ambiente `JWT__Key`

### Configuracao atual no projeto

Atualmente, o arquivo `PlaceRentalApp.API/appsettings.Development.json` ja possui um valor de desenvolvimento:

```json
"JWT": {
    "Key": "dev-super-secret-key-please-change-2026",
    "Issuer": "Place Rental App",
    "Audience": "PlaceRentalApp"
}
```

Esse valor resolve a execucao local, mas deve ser trocado por um segredo proprio em cada ambiente.

Se mais de uma fonte existir, o ASP.NET Core usa a ultima configuracao carregada com maior prioridade.

Exemplo com user secrets:

```bash
dotnet user-secrets init --project ./PlaceRentalApp.API
dotnet user-secrets set "JWT:Key" "2f8d7f6d2f0c4f8e91b7c1f0d7d9a9b1c8e4f2b7a6d1f0c9e8a7b6c5d4e3f2a1" --project ./PlaceRentalApp.API
```

Se o projeto nao tiver `UserSecretsId` no `.csproj`, o comando pode falhar. Nesse caso, use `appsettings.Development.json` ou variavel de ambiente.

Exemplo com variavel de ambiente:

```bash
export JWT__Key="2f8d7f6d2f0c4f8e91b7c1f0d7d9a9b1c8e4f2b7a6d1f0c9e8a7b6c5d4e3f2a1"
```

## 1.1 Aplicando a migracao do banco

Quando o JWT passou a depender de `Password` e `Role` no usuario, foi necessario atualizar o banco com a migracao abaixo:

```bash
dotnet ef database update --project ./PlaceRentalApp.Infrastructure -s ./PlaceRentalApp.API --context PlaceRentalDbContext
```

Use esse comando a partir da raiz da solution. Ele aplica a migracao que adiciona as colunas novas na tabela `Users` e evita o erro `Invalid column name 'Password'` ao consultar rotas que carregam o usuario relacionado, como `GET /api/places?search=casa`.

## 2. Registro da autenticacao

O projeto registra o `JwtBearer` dentro do modulo de infraestrutura:

```csharp
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWT:Issuer"],
            ValidAudience = configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                JwtKeyHelper.GetSigningKeyBytes(configuration)
            )
        };
    });
```

O que cada parametro faz:

- `ValidateIssuer`: confere se o emissor do token bate com o configurado.
- `ValidateAudience`: confere se o publico do token bate com o configurado.
- `ValidateLifetime`: rejeita tokens expirados.
- `ValidateIssuerSigningKey`: garante que a assinatura foi criada com a chave correta.
- `ValidIssuer`: valor esperado para o emissor.
- `ValidAudience`: valor esperado para o publico.
- `IssuerSigningKey`: chave simetrica usada para verificar a assinatura.

### Validacao compartilhada da chave

O projeto usa `JwtKeyHelper.GetSigningKeyBytes(configuration)` para validar a chave em um unico lugar.

Essa ajuda evita dois problemas comuns:

- chave ausente em `JWT:Key`
- chave muito curta para `HS256`

Detalhe de implementacao: o helper converte a chave para bytes com `Encoding.UTF8.GetBytes(key)` e valida se o tamanho final e >= 16 bytes.

Quando o segredo nao atende ao minimo, a aplicacao levanta uma excecao mais clara antes de chegar no erro criptografico do pacote JWT.

## 3. Geracao do token

O `AuthService` cria o JWT com claims, emissor, audiencia e expiracao:

```csharp
public string GenerateToken(string email, string role)
{
    var issuer = _configuration["JWT:Issuer"];
    var audience = _configuration["JWT:Audience"];

    var key = new SymmetricSecurityKey(
        JwtKeyHelper.GetSigningKeyBytes(_configuration)
    );

    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim("userName", email),
        new Claim(ClaimTypes.Role, role)
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        expires: DateTime.UtcNow.AddMinutes(120),
        signingCredentials: credentials,
        claims: claims
    );

    var handler = new JwtSecurityTokenHandler();
    return handler.WriteToken(token);
}
```

### O que vai dentro do token

- `userName`: identifica o usuario logado.
- `ClaimTypes.Role`: guarda a role do usuario para autorizacao.
- `expires`: define o tempo de vida do token.

### Observacao importante

O nome da chave de configuracao precisa ser `JWT:Issuer`. Se houver divergencia entre `JWT:Issuer` e o valor lido no codigo, o token pode ser gerado com um emissor diferente do esperado pela validacao.

O mesmo vale para `JWT:Key`: o valor precisa ser consistente entre geracao e validacao, e longo o bastante para o algoritmo `HS256`.

## 4. Login no service

O `UserService` faz o login em duas etapas: valida a senha e gera o token.

```csharp
public ResultViewModel<LoginViewModel> Login(LoginInputModel model)
{
    string hash = _authService.ComputeHash(model.Password);

    User? user = _userRepository.GetUserByAuth(model.Email, hash);

    if (user is null)
        return (ResultViewModel<LoginViewModel>)ResultViewModel.Error("User not found");

    string token = _authService.GenerateToken(user.Email, user.Role);

    LoginViewModel viewModel = new LoginViewModel(token);

    return ResultViewModel<LoginViewModel>.Success(viewModel);
}
```

### Entrada e saida

O input e simples:

```csharp
public class LoginInputModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

E a resposta devolve o token:

```csharp
public class LoginViewModel
{
    public LoginViewModel(string token) => Token = token;

    public string Token { get; set; }
}
```

## 5. Exemplo de endpoint de login

O projeto ja possui a regra de login no service. Para expor isso na API, o controller pode seguir este formato:

```csharp
[HttpPost("login")]
public IActionResult Login(LoginInputModel model)
{
    var result = _userService.Login(model);

    if (!result.IsSuccess)
        return BadRequest(result);

    return Ok(result);
}
```

Esse endpoint devolve um `ResultViewModel<LoginViewModel>` com o token JWT dentro de `Data`.

## 6. Pipeline da API

No `Program.cs`, a ordem do pipeline precisa incluir autenticacao antes de autorizacao:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Sem `UseAuthentication()`, o ASP.NET Core nao processa o token enviado no header `Authorization`.

### Como isso aparece no projeto

No arquivo `PlaceRentalApp.API/Program.cs`, o pipeline esta configurado nesta ordem:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Essa ordem importa porque:

- `UseAuthentication()` le o token JWT do header `Authorization` e monta o usuario autenticado.
- `UseAuthorization()` usa esse usuario para verificar atributos como `[Authorize]`.

Se a ordem for invertida, a autorizacao roda antes da autenticacao e os endpoints protegidos podem falhar mesmo com um token valido.

## 7. Swagger configurado para Bearer

A configuracao do Swagger no projeto serve para duas coisas:

1. Mostrar a definicao de seguranca do tipo Bearer.
2. Permitir que o Swagger UI envie o token JWT nas requisicoes.

### Configuracao atual

```csharp
c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlaceRentalApp.API", Version = "v1" });

c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header usando o esquema Bearer."
});

c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
{
    {
        new OpenApiSecuritySchemeReference("Bearer", null!, null!),
        new List<string>()
    }
});
```

### O que cada parte faz

#### `SwaggerDoc("v1", ...)`

Cria a documentacao principal da API com titulo e versao.

#### `AddSecurityDefinition("Bearer", ...)`

Registra uma definicao de seguranca chamada `Bearer`. Isso diz ao Swagger que a API usa um header `Authorization` com token JWT.

Campos importantes:

- `Name = "Authorization"`: nome do header.
- `Type = SecuritySchemeType.ApiKey`: Swagger trata o token como um valor enviado no header.
- `Scheme = "Bearer"`: formato textual usado no header.
- `BearerFormat = "JWT"`: ajuda a documentar o tipo do token.
- `In = ParameterLocation.Header`: informa que o valor fica no header.
- `Description`: texto de apoio exibido na UI.

#### `AddSecurityRequirement(...)`

Indica que os endpoints devem considerar a definicao `Bearer` como requisito de seguranca.

No projeto, a versao instalada do `Microsoft.OpenApi` espera:

- `OpenApiSecuritySchemeReference` como chave
- `List<string>` como valor
- um factory lambda para montar o requisito

Isso e o motivo de a configuracao nao usar mais o padrao antigo com `OpenApiReference`.

### Como usar no Swagger UI

Depois de gerar um token:

1. Abra o Swagger.
2. Clique em `Authorize`.
3. Cole o valor no formato `Bearer <token>`.
4. Execute os endpoints protegidos.

O Swagger passara o token no header `Authorization` automaticamente.

## 8. Exemplo completo do fluxo

1. Um usuario se registra via `POST /api/users`.
2. A senha e salva como hash.
3. O usuario chama o endpoint de login com email e senha.
4. O service encontra o usuario e gera o JWT.
5. O cliente copia o token.
6. O cliente abre o Swagger e usa `Authorize`.
7. O endpoint protegido valida assinatura, issuer, audience e expiracao.

## 9. Erros comuns

- Usar `app.UseAuthorization()` sem `app.UseAuthentication()`.
- Configurar uma chave `JWT:Issuer` no `appsettings.json` e ler outro nome no codigo.
- Usar uma `JWT:Key` curta demais para `HS256`.
- Esquecer de manter `IssuerSigningKey` igual na geracao e na validacao.
- Enviar o token sem o prefixo `Bearer`.
- Marcar endpoints com `[Authorize]` sem registrar `AddAuthentication().AddJwtBearer(...)`.

## 10. Resumo pratico

Se quiser implementar JWT nesse projeto, o essencial e:

1. Manter `JWT:Key`, `JWT:Issuer` e `JWT:Audience` no `appsettings.json`.
2. Garantir que `JWT:Key` tenha pelo menos 16 bytes.
3. Registrar `AddAuthentication().AddJwtBearer(...)` no modulo de infraestrutura.
4. Gerar o token com `JwtSecurityToken` no `AuthService`.
5. Chamar `app.UseAuthentication()` antes de `app.UseAuthorization()`.
6. Configurar o Swagger com a definicao Bearer para facilitar testes.

Esse conjunto fecha o ciclo completo de autenticacao JWT no PlaceRentalApp.
