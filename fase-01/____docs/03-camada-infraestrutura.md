# 03. Camada de Infraestrutura (Infrastructure Layer)

#infrastructure #efcore #sqlserver #azuresql #repository-pattern #migrations

Voltar para a [[index|Visão Geral]] | Ver anterior: [[02-camada-dominio|02. Camada de Domínio]]

---

## 🎯 Objetivo

A camada de **Infraestrutura (`_02_infrastructure`)** é responsável por implementar o acesso a dados persistence com o Entity Framework Core, o mapeamento de tabelas via Fluent API, o gerenciamento de transações/banco de dados e a resiliência de rede ao conectar com serviços em nuvem como o Azure SQL Server.

---

## 📂 Estrutura de Pastas de Infraestrutura

```
_02_infrastructure/
├── data/
│   ├── AppDbContext.cs
│   └── migrations/
│       ├── 20260815185159_InitialCreate.cs
│       ├── 20260815222757_AddPhotos.cs
│       └── AppDbContextModelSnapshot.cs
└── repositories/
    ├── BaseRepository.cs
    ├── GamePhotoRepository.cs
    ├── GameRepository.cs
    ├── UserPhotoRepository.cs
    └── UserRepository.cs
```

---

## ⚙️ Mapeamento com Fluent API (`AppDbContext.cs`)

O `AppDbContext` herda de `DbContext` e configura o mapeamento relacional dentro de `OnModelCreating`.

```csharp
namespace fase_01.infrastructure.data
{
    using Microsoft.EntityFrameworkCore;
    using fase_01.domain.entities;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();
        public DbSet<GamePhoto> GamePhotos => Set<GamePhoto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamento da Entidade User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.NickName).HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Admin).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ValidatedAt);

                // Constraint de validação a nível de banco
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Users_ValidatedAt_GreaterThan_CreatedAt",
                    "[ValidatedAt] IS NULL OR [ValidatedAt] >= [CreatedAt]"
                ));
            });

            // Mapeamento da Entidade Game
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Manufacturer).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ReleasedAt);
                entity.Property(e => e.Description);
                entity.Property(e => e.Online).IsRequired();
                entity.Property(e => e.Multiplayer).IsRequired();
                entity.Property(e => e.CategoryId).IsRequired();
                entity.Property(e => e.UrlGame).HasMaxLength(255);
                entity.Property(e => e.UrlVideo).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Ignora o Smart Enum no Mapeamento do EF (persistimos apenas o CategoryId)
                entity.Ignore(e => e.Category);
            });

            // Mapeamento de GamePhoto (Relacionamento 1 <-> 0..1 com Cascade Delete)
            modelBuilder.Entity<GamePhoto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.Thumbnail);
                entity.HasOne(e => e.Game)
                      .WithOne()
                      .HasForeignKey<GamePhoto>(e => e.Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
```

---

## 🏛️ Implementação do Padrão Repository

### 1. Repositório Base Genérico (`BaseRepository.cs`)
Fornece todas as operações assíncronas padrão reutilizando o `DbSet<T>`:

```csharp
namespace fase_01.infrastructure.repositories
{
    using Microsoft.EntityFrameworkCore;
    using fase_01.domain.interfaces;
    using fase_01.infrastructure.data;

    public abstract class BaseRepository<T, TKey> : IRepositoryBase<T, TKey> where T : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<T> DbSet;

        protected BaseRepository(AppDbContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public virtual async Task AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
                await Context.SaveChangesAsync();
            }
        }

        public virtual async Task<T?> GetByIdAsync(TKey id) => await DbSet.FindAsync(id);

        public virtual async Task<IEnumerable<T>> ListAllAsync() => await DbSet.ToListAsync();

        public virtual async Task UpSertAsync(T entity)
        {
            // UpSert: Insere se não existir, atualiza se já existir
            DbSet.Update(entity);
            await Context.SaveChangesAsync();
        }
    }
}
```

### 2. Repositórios Concretos
Exemplo do `GameRepository.cs`:

```csharp
namespace fase_01.infrastructure.repositories
{
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;
    using fase_01.infrastructure.data;

    public class GameRepository : BaseRepository<Game, int>, IGameRepository
    {
        public GameRepository(AppDbContext context) : base(context) { }
    }
}
```

---

## ☁️ Resiliência e Conexão Azure SQL Server (`EnableRetryOnFailure`)

Bancos de dados em nuvem como o **Azure SQL Server (Serverless)** podem entrar em pausa automática após períodos de inatividade. Quando uma requisição é feita, a primeira tentativa pode falhar com o erro `40613` (Database not currently available).

Para evitar que a aplicação quebre nessas situações transitórias, configuramos a **Resiliência de Tentativas** no `Program.cs`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                       // Número máximo de tentativas (5x)
            maxRetryDelay: TimeSpan.FromSeconds(10),// Intervalo máximo entre tentativas
            errorNumbersToAdd: null                 // Adiciona lista interna padrão de erros transitórios
        );
    })
);
```

---

## 🔄 Gerenciamento de Migrations com EF Core CLI

### Criar uma nova migration:
```bash
dotnet ef migrations add NomeDaMigration
```

### Aplicar as alterações ao banco de dados:
```bash
dotnet ef database update
```

---

Próximo passo: [[04-camada-aplicacao|04. Camada de Aplicação]]
