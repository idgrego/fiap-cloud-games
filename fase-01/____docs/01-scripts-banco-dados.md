# 01. Scripts e Estrutura de Banco de Dados

#sqlserver #azuresql #database #ddl #dml

Voltar para a [[index|Visão Geral]] | Ver anterior: [[00-modelagem-ddd-event-storming|00. Modelagem DDD]] | Ver próximo: [[02-camada-dominio|02. Camada de Domínio]]

---

## 🎯 Objetivo

Esta etapa documenta a modelagem relacional das tabelas de banco de dados do **FIAP Cloud Games**, os relacionamentos de chave primária/estrangeira, regras de integridade e os scripts SQL DDL/DML utilizados para provisionamento inicial.

---

## 📂 Localização dos Scripts no Projeto

Os scripts SQL estão organizados no diretório `_00_scripts-sql/`:

- `_01_users.sql`: Criação das tabelas de usuários e fotos de usuários.
- `_03_games.sql`: Criação da tabela de categorias, jogos e fotos de jogos.
- `_04_games_inserts.sql`: Carga de dados reais de jogos dos consoles Xbox, PlayStation 5 e Nintendo Switch.

---

## 📐 Estrutura Relacional (Modelo DDL)

### 1. Tabela de Usuários (`Users`, `Accounts` e `UsersPhotos`)

```sql
CREATE TABLE Users (
    id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    fullname VARCHAR(255) NOT NULL,
    nickname VARCHAR(50) NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    admin BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP,
    validated_at DATETIME2 NULL,
    CONSTRAINT CK_Users_ValidatedAt_GreaterThan_CreatedAt CHECK (validated_at IS NULL OR validated_at >= created_at)
);

CREATE TABLE Accounts (
    id INT NOT NULL PRIMARY KEY REFERENCES Users(id) ON DELETE CASCADE,
    password_hash VARCHAR(255) NOT NULL,
    approved BIT NOT NULL DEFAULT 0,
    failed_counter INT NOT NULL DEFAULT 0
);

CREATE TABLE UsersPhotos (
    id INT NOT NULL PRIMARY KEY REFERENCES Users(id) ON DELETE CASCADE,
    content_type VARCHAR(50) NOT NULL,
    image VARBINARY(MAX) NOT NULL,
    thumbnail VARBINARY(MAX) NULL
);
```

> [!NOTE] Destaque de Modelagem: Tabela de Credenciais (`Accounts`) e Fotos (`UsersPhotos`)
> Tanto a chave primária `id` em `Accounts` quanto em `UsersPhotos` são Chaves Estrangeiras apontando para `Users(id)`. Isso garante a cardinalidade **1 para 0..1** (um usuário possui no máximo uma conta com hash de senha e no máximo uma foto de perfil). O `ON DELETE CASCADE` garante a exclusão automática de credenciais e mídia ao remover um usuário.

---

### 2. Tabela de Jogos (`Games`, `EnumGamesCategories` e `GamesPhotos`)

```sql
CREATE TABLE EnumGamesCategories (
    id TINYINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO EnumGamesCategories (name) VALUES
    ('Unknown'), ('Action'), ('Adventure'), ('Role-Playing'),
    ('Simulation'), ('Strategy'), ('Sports'), ('Puzzle'), ('Racing'), ('Fighting'), ('Horror');

CREATE TABLE Games (
    id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(255) NOT NULL,
    manufacturer VARCHAR(255) NOT NULL,
    released_at DATE NULL,
    description VARCHAR(MAX) NULL,
    online BIT NOT NULL DEFAULT 0,
    multiplayer BIT NOT NULL DEFAULT 0,
    category_id TINYINT NOT NULL REFERENCES EnumGamesCategories(id) DEFAULT 1,
    url_game VARCHAR(255) NULL,
    url_video VARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE GamesPhotos (
    id INT NOT NULL PRIMARY KEY REFERENCES Games(id) ON DELETE CASCADE,
    content_type VARCHAR(50) NOT NULL,
    image VARBINARY(MAX) NOT NULL,
    thumbnail VARBINARY(MAX) NULL
);
```

---

## ⚡ Script de Carga de Dados (DML Seed)

O arquivo `_04_games_inserts.sql` popula o banco de dados com jogos reais das principais plataformas do mercado (Xbox, PlayStation 5 e Nintendo Switch), englobando diversas categorias (Ação, Aventura, RPG, Simulação, Estratégia, Esportes, Puzzle, Corrida, Luta e Horror).

Exemplo de trecho de inserção:

```sql
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Halo Infinite', 'Xbox Game Studios', 1, 'O Lendário Master Chief retorna na maior aventura já criada da franquia Halo para salvar a humanidade.', 1, 1, '2021-12-08', 'https://www.xbox.com/pt-BR/games/halo-infinite', 'https://www.youtube.com/watch?v=PyMlV5_HRWk', GETUTCDATE()),
('God of War Ragnarök', 'Sony Interactive Entertainment', 1, 'Kratos e Atreus embarcam em uma jornada épica em busca de respostas nos Nove Reinos antes do Ragnarök.', 0, 0, '2022-11-09', 'https://www.playstation.com/pt-br/games/god-of-war-ragnarok/', 'https://www.youtube.com/watch?v=hfJ4Km46A-0', GETUTCDATE()),
('Super Mario Odyssey', 'Nintendo', 1, 'Junte-se a Mario em uma enorme aventura 3D pelo mundo todo e use suas novas habilidades para salvar a Princesa Peach.', 0, 1, '2017-10-27', 'https://www.nintendo.com/pt-br/store/products/super-mario-odyssey-switch/', 'https://www.youtube.com/watch?v=wGQHQc_3yYo', GETUTCDATE());
```

---

## 🛠️ Como Executar os Scripts

1. Abra a extensão do SQL Server / Azure Data Studio no VS Code ou use a ferramenta `sqlcmd`.
2. Conecte-se à sua instância local ou ao **Azure SQL Server** (`fcg-server.database.windows.net`).
3. Certifique-se de estar usando o banco `fcg-db`.
4. Execute os arquivos em ordem:
   - `_01_users.sql`
   - `_03_games.sql`
   - `_04_games_inserts.sql`

---

Próximo passo: [[02-camada-dominio|02. Camada de Domínio]]
