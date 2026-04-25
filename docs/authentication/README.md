# Autenticacao JWT - Indice

Documentacao sobre como o JWT esta montado no projeto PlaceRental.

## Documentos

### [JWT_AUTHENTICATION.md](./JWT_AUTHENTICATION.md) - Guia completo

Explica o fluxo de autenticacao completo:

- configuracao em `appsettings.json`
- onde definir a chave JWT em desenvolvimento
- aplicacao da migracao do banco
- validacao do segredo JWT e tamanho minimo da chave
- validacao da chave por bytes UTF-8
- ordem do pipeline com `UseAuthentication()` e `UseAuthorization()`
- registro do `JwtBearer` no container de DI
- geracao do token em `AuthService`
- consumo no `UserService`
- configuracao detalhada do Swagger com Bearer
- pontos de atencao e erros comuns

---

**Tempo total de leitura**: ~15 minutos  
**Nivel**: Intermediario
