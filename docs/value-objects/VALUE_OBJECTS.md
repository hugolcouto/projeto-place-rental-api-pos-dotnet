# 💎 Value Objects - Guia Completo

## Introdução

**Value Objects** são objetos que não têm identidade própria, apenas valor. Diferem das **Entities** que têm ID único.

### 🔍 Diferença: Entity vs Value Object

```
ENTITY                          VALUE OBJECT
─────────────────────────────   ─────────────────────────────
Tem ID único                    Sem ID único
Place com Id = 1                Address "Rua A, 100"

Podem ser mutáveis              Imutáveis

Têm ciclo de vida               Não têm ciclo de vida

Comportam estado                Apenas representam valor

┌─────────────┐                 ┌──────────────┐
│  Place      │                 │  Address     │
├─────────────┤                 ├──────────────┤
│ Id = 1      │  (Tem ID)       │ Street=Rua A │ (Sem ID)
│ Title=Casa  │                 │ Number=100   │
│ Address=... │                 │ City=SP      │
└─────────────┘                 └──────────────┘
```

---

## 💡 Exemplo no Projeto: Address

### A Classe Address

```csharp
public class Address
{
    // ✅ Construtor com todos os parâmetros
    public Address(
        string street,
        string number,
        string zipCode,
        string district,
        string city,
        string state,
        string country)
    {
        Street = street;
        Number = number;
        ZipCode = zipCode;
        District = district;
        City = city;
        State = state;
        Country = country;
    }

    // ✅ Propriedades readonly (imutáveis)
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string ZipCode { get; private set; }
    public string District { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }

    // ✅ Sem ID
    // ✅ Sem ciclo de vida
}
```

---

### Usando em uma Entity

```csharp
public class Place : BaseEntity
{
    // ✅ Address como propriedade (value object)
    public Address Address { get; private set; }

    public Place(
        string title,
        string description,
        decimal dailyPrice,
        Address address,  // ✅ Recebe como parâmetro
        int allowedNumberPerson,
        bool allowPets,
        string createdBy)
    {
        Title = title;
        Description = description;
        DailyPrice = dailyPrice;
        Address = address;  // ✅ Armazena
        // ...
    }

    // ✅ Não pode alterar Address diretamente
    // place.Address.Street = "Rua B"; // ❌ Erro!
    // Precisa criar um novo Address
}
```

---

## 📦 Mapeando com Entity Framework

### OwnsOne - Posse Completa

```csharp
// No OnModelCreating do DbContext
builder.Entity<Place>(e =>
{
    // ✅ Address é possuído por Place
    e.OwnsOne(p => p.Address, a =>
    {
        // Configurar colun as
        a.Property(d => d.Street).HasColumnName("Street");
        a.Property(d => d.Number).HasColumnName("Number");
        a.Property(d => d.ZipCode).HasColumnName("ZipCode");
        a.Property(d => d.District).HasColumnName("District");
        a.Property(d => d.City).HasColumnName("City");
        a.Property(d => d.State).HasColumnName("State");
        a.Property(d => d.Country).HasColumnName("Country");
    });
});

// No banco de dados:
// Tabela: Places
// Colunas: Id, Title, Description, Street, Number, City, State, ...
// (Não há tabela separada de Address!)
```

---

## ✨ Características de Value Objects

### 1. Imutabilidade

```csharp
// ✅ Value Object imutável
public class Address
{
    public string Street { get; private set; } // private setter
}

var address = new Address("Rua A", "100", ...);

// ❌ Não pode modificar
// address.Street = "Rua B"; // Erro de compilação!

// ✅ Criar novo
var newAddress = new Address("Rua B", "100", ...);
```

---

### 2. Igualdade por Valor

```csharp
var address1 = new Address("Rua A", "100", ...);
var address2 = new Address("Rua A", "100", ...);

// ❌ Sem override: false (são objetos diferentes)
bool equals1 = address1 == address2; // false!

// ✅ Com override: true (mesmo valor)
public override bool Equals(object obj)
{
    if (obj is not Address other) return false;
    return Street == other.Street &&
           Number == other.Number;
}

bool equals2 = address1 == address2; // true!
```

---

### 3. Sem ID Único

```csharp
// Address NÃO tem ID
public class Address
{
    // ❌ Sem isso
    // public int Id { get; set; }

    public string Street { get; private set; }
    public string City { get; private set; }
}

// Place TEM ID
public class Place : BaseEntity
{
    public int Id { get; private set; }  // ✅ Tem ID
    public Address Address { get; private set; } // ✅ Não tem ID
}
```

---

## 🎯 Quando Usar Value Objects

### ✅ Use Value Object

```csharp
// ✅ Email
public class Email
{
    public string Value { get; private set; }

    public Email(string value)
    {
        if (!value.Contains("@"))
            throw new ArgumentException("Email inválido");
        Value = value;
    }
}

// ✅ Moeda
public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
}

// ✅ Localização
public class Location
{
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
}

// ✅ Periodo de Tempo
public class DateRange
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
}
```

---

### ❌ Não Use Value Object

```csharp
// ❌ Não: Precisa ser identificado individualmente
public class Comment // Deveria ser Entity!
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ❌ Não: Tem ciclo de vida próprio
public class Order // Deveria ser Entity!
{
    public int OrderId { get; set; }
    public DateTime CreatedDate { get; set; }
    public OrderStatus Status { get; set; }
}
```

---

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Imutável
public class Address
{
    public string Street { get; private set; }
}

// ✅ Validação no construtor
public class Email
{
    public Email(string value)
    {
        if (!value.Contains("@"))
            throw new ArgumentException();
        Value = value;
    }
    public string Value { get; private set; }
}

// ✅ Usar OwnsOne
builder.Entity<Place>().OwnsOne(p => p.Address);

// ✅ Override Equals
public override bool Equals(object obj) { ... }
```

### ❌ DON'T

```csharp
// ❌ Mutável
public class Address
{
    public string Street { get; set; } // ❌ public setter!
}

// ❌ Sem validação
public class Email
{
    public string Value { get; set; } // Qualquer coisa!
}

// ❌ Adicionar ID
public class Address
{
    public int Id { get; set; } // ❌ Value objects não têm ID!
}

// ❌ Herdar de Entity
public class Address : BaseEntity { } // ❌ Errado!
```

---

## 🎓 Conclusão

Value Objects são essenciais em Domain-Driven Design:

- 🔒 Encapsulam conceitos pequenos
- ✨ São imutáveis
- 🧪 São fáceis de testar
- 📚 Melhoram a linguagem do domínio

**No PlaceRental**, `Address` é um Value Object que encapsula a localização de um imóvel!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
