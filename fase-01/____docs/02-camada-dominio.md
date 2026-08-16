# 02. Camada de Domínio (Domain Layer)

#domain #csharp #smartenum #entities #repository-pattern

Voltar para a [[index|Visão Geral]] | Ver anterior: [[01-scripts-banco-dados|01. Scripts de Banco de Dados]]

---

## 🎯 Objetivo

A camada de **Domínio (`_01_domain`)** é o coração da aplicação. Ela contém as regras de negócio puras, entidades de domínio, os valores de domínio (como os *Smart Enums*) e as interfaces de contrato dos repositórios.

> [!IMPORTANT] Princípio Arquitetural
> A camada de Domínio **não possui nenhuma dependência** com frameworks de infraestrutura (como Entity Framework Core ou ASP.NET Core). Ela define *o que* o sistema faz através de interfaces C#, deixando para a camada de Infraestrutura a implementação de *como* o dado é persistido.

---

## 🗂️ Estrutura de Pastas de Domínio

```
_01_domain/
├── entities/
│   ├── Game.cs
│   ├── GamePhoto.cs
│   ├── PhotoBase.cs
│   ├── User.cs
│   └── UserPhoto.cs
├── enums/
│   └── GameCategory.smart.cs
└── interfaces/
    ├── IGamePhotoRepository.cs
    ├── IGameRepository.cs
    ├── IRepositoryBase.cs
    ├── IUserPhotoRepository.cs
    └── IUserRepository.cs
```

---

## 🧩 Entidades do Domínio

### 1. `User.cs`
Representa um usuário do sistema com suporte a perfis de administrador e data de validação de e-mail.

```csharp
namespace fase_01.domain.entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool Admin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ValidatedAt { get; set; }
    }
}
```

### 2. `Game.cs` e Integração com Smart Enum
A entidade `Game` armazena o identificador numérico da categoria (`CategoryId`) e expõe uma propriedade navegável inteligente `Category`:

```csharp
using fase_01.domain.enums;

namespace fase_01.domain.entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Online { get; set; }
        public bool Multiplayer { get; set; }
        public DateOnly? ReleasedAt { get; set; }

        public byte CategoryId { get; set; }
        private GameCategory _category = GameCategory.Unknown;
        
        public GameCategory Category
        {
            get
            {
                if (_category.Code != CategoryId)
                    _category = GameCategory.FromCode(CategoryId);
                return _category;
            }
            set
            {
                _category = value ?? GameCategory.Unknown;
                CategoryId = _category.Code;
            }
        }

        public string? UrlGame { get; set; }
        public string? UrlVideo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### 3. Hierarquia de Fotos (`PhotoBase`, `GamePhoto`, `UserPhoto`)
Para reaproveitamento da estrutura binária de imagens, foi criada a classe abstrata `PhotoBase`:

```csharp
namespace fase_01.domain.entities
{
    public abstract class PhotoBase
    {
        public int Id { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public byte[]? Thumbnail { get; set; }
    }

    public class GamePhoto : PhotoBase
    {
        public virtual Game Game { get; set; } = null!;
    }

    public class UserPhoto : PhotoBase
    {
        public virtual User User { get; set; } = null!;
    }
}
```

---

## 🌟 O Padrão Smart Enum (`GameCategory.smart.cs`)

### Por que usar Smart Enum em vez de `enum` tradicional C#?
Os `enums` primitivos do C# (como `enum Category { Action = 1 }`) apenas associam um nome a um inteiro. Eles possuem limitações:
- Não permitem adicionar métodos estáticos ricos de busca (`FromCode`, `FromName`).
- Não possuem validação forte na atribuição de valores inválidos sem castings perigosos.
- Não permitem descrições legíveis ricas diretamente.

O padrão **Smart Enum** (Enum como Classe) resolve isso encapsulando o código `byte` e o nome legível:

```csharp
namespace fase_01.domain.enums
{
    public class GameCategory
    {
        public readonly byte Code;
        public readonly string Name;

        private GameCategory(byte code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        public static readonly GameCategory Unknown = new(0, "Unknown");
        public static readonly GameCategory Action = new(1, "Action");
        public static readonly GameCategory Adventure = new(2, "Adventure");
        public static readonly GameCategory RolePlaying = new(3, "RolePlaying");
        public static readonly GameCategory Simulation = new(4, "Simulation");
        public static readonly GameCategory Strategy = new(5, "Strategy");
        public static readonly GameCategory Sports = new(6, "Sports");
        public static readonly GameCategory Puzzle = new(7, "Puzzle");
        public static readonly GameCategory Racing = new(8, "Racing");
        public static readonly GameCategory Fighting = new(9, "Fighting");
        public static readonly GameCategory Horror = new(10, "Horror");

        public static IEnumerable<GameCategory> List() =>
            [Unknown, Action, Adventure, RolePlaying, Simulation, Strategy, Sports, Puzzle, Racing, Fighting, Horror];

        public static GameCategory FromCode(byte code)
        {
            return code switch
            {
                1 => Action, 2 => Adventure, 3 => RolePlaying, 4 => Simulation,
                5 => Strategy, 6 => Sports, 7 => Puzzle, 8 => Racing,
                9 => Fighting, 10 => Horror, _ => Unknown
            };
        }
    }
}
```

---

## 📋 Interfaces de Repositório (`_01_domain/interfaces/`)

### 1. Repositório Base Genérico (`IRepositoryBase<T, TKey>`)
Define o CRUD padrão reutilizável por qualquer entidade:

```csharp
namespace fase_01.domain.interfaces
{
    public interface IRepositoryBase<T, TKey> where T : class
    {
        Task UpSertAsync(T entity);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(TKey id);
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> ListAllAsync();
    }
}
```

### 2. Interfaces Específicas
As interfaces filhas herdam de `IRepositoryBase` passando os tipos concretos:

```csharp
public interface IGameRepository : IRepositoryBase<Game, int> { }
public interface IUserRepository : IRepositoryBase<User, int> { }
public interface IGamePhotoRepository : IRepositoryBase<GamePhoto, int> { }
public interface IUserPhotoRepository : IRepositoryBase<UserPhoto, int> { }
```

---

Próximo passo: [[03-camada-infraestrutura|03. Camada de Infraestrutura]]
