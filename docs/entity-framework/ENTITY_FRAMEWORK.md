# 🗄️ Entity Framework - Guia Completo

## Introdução

**Entity Framework Core (EF Core)** é um ORM (Object-Relational Mapper) que permite trabalhar com banco de dados usando objetos .NET em vez de escrever SQL puro.

### 🎯 Objetivo

- ✅ Mapear tabelas para classes .NET
- ✅ Abstrair consultas SQL
- ✅ Gerenciar relacionamentos
- ✅ Rastrear mudanças

---

## 🔑 Conceitos Básicos

### O Que é ORM?

```
Banco de Dados (SQL)          .NET (Objetos)
┌──────────────────┐          ┌──────────────────┐
│ Tabela: Places   │   ←→    │ Class: Place     │
├──────────────────┤          ├──────────────────┤
│ id (int)         │          │ Id (int)         │
│ title (varchar)  │          │ Title (string)   │
│ price (decimal)  │          │ DailyPrice (decimal)
└──────────────────┘          └──────────────────┘

Você trabalha com objetos!
Entity Framework converte para SQL!
```

---

## 📦 DbContext - O Coração do EF

### O Que é DbContext?

É a classe que:

- Representa a conexão com o banco
- Mantém o mapeamento de entidades
- Rastreia mudanças
- Executa consultas

### No PlaceRental

```csharp
public class PlaceRentalDbContext : DbContext
{
    // Construtor: recebe opções de configuração
    public PlaceRentalDbContext(
        DbContextOptions<PlaceRentalDbContext> options
    ) : base(options) { }

    // DbSets: Representam tabelas no banco
    public DbSet<Place> Places { get; set; }
    public DbSet<PlaceAmenity> PlaceAmenities { get; set; }
    public DbSet<PlaceBook> PlaceBooks { get; set; }
    public DbSet<User> Users { get; set; }

    // OnModelCreating: Configuração do mapeamento
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configurar entidades aqui
    }
}
```

---

## 🔗 Tipos de Relacionamentos

### 1. One-to-Many (Um para Muitos)

```csharp
// Place tem muitos PlaceBooks
public class Place : BaseEntity
{
    public string Title { get; private set; }

    // ✅ Coleção de books
    public List<PlaceBook> Books { get; private set; } = new();
}

public class PlaceBook : BaseEntity
{
    public int IdPlace { get; private set; }

    // ✅ Referência ao place
    public Place Place { get; private set; }
}

// Configuração no OnModelCreating
builder.Entity<Place>(e =>
{
    e.HasMany(p => p.Books)           // Place tem muitos
        .WithOne(b => b.Place)         // PlaceBook tem um
        .HasForeignKey(a => a.IdPlace) // Chave estrangeira
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 2. Value Objects (OwnsOne)

```csharp
// Place TEM UM Address (não é entidade separada)
public class Place : BaseEntity
{
    public Address Address { get; private set; } // ✅ Value Object
}

public class Address
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string City { get; private set; }
}

// Configuração
builder.Entity<Place>(e =>
{
    // ✅ Address é possuído por Place
    e.OwnsOne(p => p.Address, a =>
    {
        a.Property(d => d.Street).HasColumnName("Street");
        a.Property(d => d.Number).HasColumnName("Number");
        a.Property(d => d.City).HasColumnName("City");
    });
});

// No banco de dados:
// Tabela Places tem colunas: Street, Number, City
// (não há tabela separada de Address)
```

---

## 📝 Queries com LINQ

### Buscar Todos

```csharp
// ✅ LINQ
var places = _context.Places.ToList();

// Gerado em SQL (aproximado):
// SELECT * FROM Places;
```

---

### Buscar com Filtro

```csharp
// ✅ LINQ
var searchedPlaces = _context.Places
    .Where(p => p.Title.Contains("Casa"))
    .ToList();

// Gerado em SQL:
// SELECT * FROM Places WHERE Title LIKE '%Casa%';
```

---

### Buscar com Include (Eager Loading)

```csharp
// ✅ Com Include para carregar relacionamentos
var places = _context.Places
    .Include(p => p.User)        // Carrega User
    .Include(p => p.Books)       // Carrega Books
    .Where(p => !p.IsDeleted)
    .ToList();

// Gerado em SQL (multi-query):
// SELECT ... FROM Places JOIN Users ...
// SELECT ... FROM PlaceBooks WHERE IdPlace IN (...)
```

---

### Buscar com SingleOrDefault

```csharp
// ✅ Buscar um único resultado
var place = _context.Places.SingleOrDefault(p => p.Id == 1);

if (place == null)
{
    // Não encontrou
}

// Gerado em SQL:
// SELECT * FROM Places WHERE Id = 1;
```

---

## 💾 SaveChanges - Persistência

Entity Framework rastreia mudanças e persiste com `SaveChanges()`:

```csharp
// 1. Criar
var place = new Place("Casa", "Descrição", 100, ...);
_context.Places.Add(place);

// 2. Modificar
place.Title = "Nova Casa";

// 3. Salvar
_context.SaveChanges();

// ✅ EF enviará INSERT ou UPDATE para o banco
```

---

## 🚀 Migrations - Controle de Versão do Schema

Migrations rastreiam mudanças no schema do banco:

```csharp
// Criar migration
// dotnet ef migrations add InitialCreate

// Aplicar migrations
// dotnet ef database update

// Arquivo criado:
// 20260414142321_FirstMigration.cs
public partial class FirstMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Código SQL para criar tabelas
        migrationBuilder.CreateTable(
            name: "Places",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                // ...
            });
    }
}
```

---

## ✅ Boas Práticas

### ✅ DO

```csharp
// ✅ Usar DbSet para acessar tabelas
var place = _context.Places.Find(id);

// ✅ Usar Include para eager loading
var places = _context.Places.Include(p => p.User).ToList();

// ✅ Usar Where para filtrar no banco
var filtered = _context.Places.Where(p => p.DailyPrice > 100).ToList();

// ✅ DbContext com Scoped
builder.Services.AddScoped<PlaceRentalDbContext>();

// ✅ Usar migrations para versionamento
// dotnet ef migrations add FeatureName
```

### ❌ DON'T

```csharp
// ❌ ToList() antes de Where
var places = _context.Places.ToList()
    .Where(p => p.Title.Contains("Casa"))  // Filtra em memória!
    .ToList();

// ❌ Lazy loading sem Include
var place = _context.Places.First();
var user = place.User; // ❌ Query N+1!

// ❌ DbContext com Singleton
builder.Services.AddSingleton<PlaceRentalDbContext>();

// ❌ Alterar schema sem migration
// Sempre use: dotnet ef migrations add ...
```

---

## 🎓 Conclusão

Entity Framework permite:

- 🔄 Trabalhar com objetos em vez de SQL
- 📊 Gerenciar relacionamentos automaticamente
- 🚀 Rastrear mudanças
- 📝 Controlar versão do schema com migrations

**No PlaceRental**, EF Core mapeia todas as entidades e gerencia o banco SQL Server!

---

**Documento criado em**: 16 de Abril de 2026  
**Versão**: 1.0  
**Status**: ✅ Completo
