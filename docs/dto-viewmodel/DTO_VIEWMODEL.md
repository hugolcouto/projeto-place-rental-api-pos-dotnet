# DTO e ViewModel no PlaceRental

## Objetivo

DTOs e ViewModels definem o contrato da API sem expor diretamente entidades de dominio.

No PlaceRental, eles sao usados para:

- entrada de dados (`CreatePlaceInputModel`, `CreateUserInputModel`)
- saida de dados (`PlaceViewModel`, `PlaceDetailsViewModel`, `UserDetailsViewModel`)
- transformacao entre entidade e resposta HTTP

---

## O que existe no projeto hoje

### InputModels

`CreatePlaceInputModel`:

```csharp
public class CreatePlaceInputModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyPrice { get; set; }
    public AddressInputModel Address { get; set; }
    public int AllowedNumberPerson { get; set; }
    public bool AllowPets { get; set; }
    public int CreatedBy { get; set; }
}
```

`CreateUserInputModel`:

```csharp
public class CreateUserInputModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }
}
```

### ViewModels

`PlaceDetailsViewModel` e `PlaceViewModel` usam `FromEntity` para mapear `Place`.

```csharp
public static PlaceDetailsViewModel? FromEntity(Place? entity)
 => entity is null
    ? null
    : new PlaceDetailsViewModel(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.DailyPrice,
        entity.Address.GetFullAddress(),
        entity.AllowedNumberPerson,
        entity.AllowPets,
        entity.CreatedBy,
        entity.Amenities.Select(a => a.Description).ToList()
    );
```

```csharp
public static UserDetailsViewModel? FromEntity(User? entity)
=> entity is null
? null
: new UserDetailsViewModel
{
    FullName = entity.FullName,
    Email = entity.Email
};
```

---

## Fluxo de dados real

1. Controller recebe `InputModel` no corpo da request.
2. Service transforma `InputModel` em entidade de dominio.
3. Repository persiste entidade.
4. Service transforma entidade em `ViewModel`.
5. Controller retorna `ResultViewModel<T>` para o cliente.

Exemplo de entrada no controller:

```csharp
[HttpPost]
public IActionResult Post(CreatePlaceInputModel model)
{
    ResultViewModel<int> placeId = _placeService.Inset(model);

    return CreatedAtAction(nameof(GetById), new { id = placeId }, model);
}
```

Exemplo de saida no service:

```csharp
public ResultViewModel<UserDetailsViewModel?> GetById(int id)
{
    User user = _userRepository.GetById(id)!;

    return ResultViewModel<UserDetailsViewModel?>.Success(
        UserDetailsViewModel.FromEntity(user)
    )!;
}
```

---

## Exemplos de implementacao

### Exemplo 1: InputModel para entidade

```csharp
Address address = new Address(
    model.Address.Street,
    model.Address.Number,
    model.Address.ZipCode,
    model.Address.District,
    model.Address.City,
    model.Address.State,
    model.Address.Country
);

Place place = new Place(
    model.Title,
    model.Description,
    model.DailyPrice,
    address,
    model.AllowedNumberPerson,
    model.AllowPets,
    model.CreatedBy
);
```

### Exemplo 2: Entidade para ViewModel de lista

```csharp
List<PlaceViewModel>? model = availablePlaces!.Select(
    PlaceViewModel.FromEntity
).ToList()!;
```

### Exemplo 3: Entidade para ViewModel de detalhe

```csharp
return ResultViewModel<PlaceDetailsViewModel?>.Success(
    PlaceDetailsViewModel.FromEntity(place)
)!;
```

---

## Erros comuns com DTO/ViewModel

### Erro 1: retornar entidade direto no controller

Problema:

- acopla API ao modelo de dominio
- aumenta risco de expor campos internos

Melhor abordagem:

- controller retorna resultado do service
- service retorna `ResultViewModel<TViewModel>`

### Erro 2: mapeamento sem tratar nulo

Exemplo inseguro:

```csharp
var vm = PlaceViewModel.FromEntity(entity); // entity pode ser nula
```

Impacto:

- `NullReferenceException` durante serializacao ou mapeamento

No projeto, o padrao adotado nos mapeadores usa `entity is null ? null : ...`.

### Erro 3: contrato inconsistente entre service e controller

Sintoma:

- service retorna `ResultViewModel<int>`
- controller monta `CreatedAtAction` com objeto incorreto em `routeValues`

Boas praticas:

- enviar apenas o ID em `routeValues`
- padronizar nome de variavel para evitar ambiguidades (`placeId`, `userId`)

### Erro 4: InputModel sem validacao minima

Sem validacao, o service recebe dados invalidos e precisa tratar tudo manualmente.

Exemplo recomendado:

```csharp
public class CreateUserInputModel
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public DateTime BirthDate { get; set; }
}
```

---

## Boas praticas no contexto do projeto

1. InputModel para entrada, ViewModel para saida.
2. Nao expor entidade diretamente pela API.
3. Centralizar mapeamento no `FromEntity`.
4. Retornar contratos consistentes com `ResultViewModel<T>`.
5. Manter nomes claros (`CreateXInputModel`, `XDetailsViewModel`).

---

## Checklist ao criar novo endpoint

1. Criou `InputModel` de entrada?
2. Criou `ViewModel` de saida?
3. Adicionou metodo `FromEntity`?
4. Service retorna `ResultViewModel<TViewModel>`?
5. Controller evita retornar entidade direta?
6. Contrato JSON da resposta ficou estavel?

---

## Resumo

No PlaceRental, DTO/ViewModel nao e apenas organizacao de codigo: e o contrato da API.

Quando mapeamento, contrato e validacao estao alinhados, os endpoints ficam previsiveis, seguros e mais simples de manter.
