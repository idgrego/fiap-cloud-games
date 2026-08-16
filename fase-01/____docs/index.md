# FIAP Cloud Games - Documentação da Fase 1

#arquitetura #dotnet #mvc #efcore #obsidian #fiap

Bem-vindo à documentação técnica do projeto **FIAP Cloud Games (Fase 1)**. Este repositório de documentos foi estruturado para ser lido e navegado de forma fluida no **Obsidian**, utilizando links internos e organização por camadas de software.

---

## 📌 Sumário da Documentação

Abaixo estão os módulos documentados em ordem cronológica de desenvolvimento. Siga este fluxo para entender ou reproduzir o projeto do zero:

1. [[01-scripts-banco-dados|01. Scripts e Estrutura de Banco de Dados]]
   - Criação de tabelas relacionais em SQL Server / Azure SQL.
   - Script de carga e povoamento inicial de jogos (*seed data*).
2. [[02-camada-dominio|02. Camada de Domínio (Domain Layer)]]
   - Entidades de Domínio (`Game`, `User`, `GamePhoto`, `UserPhoto`).
   - Padrão *Smart Enum* para Categorias (`GameCategory`).
   - Contratos e Interfaces de Repositório (`IRepositoryBase`, `IGameRepository`, etc.).
3. [[03-camada-infraestrutura|03. Camada de Infraestrutura (Infrastructure Layer)]]
   - Configuração do Entity Framework Core (`AppDbContext`).
   - Mapeamento Fluent API, Check Constraints e Cascading Deletes.
   - Implementação do Padrão *Repository* Genérico e Específico.
   - Migrations do EF Core e Resiliência com Azure SQL (`EnableRetryOnFailure`).
4. [[04-camada-aplicacao|04. Camada de Aplicação (Application Layer)]]
   - DTOs (Data Transfer Objects) e Validações Cruzadas (`IValidatableObject`).
   - Mapeamento bidirecional via Métodos de Extensão C# (`ToDto` e `ToEntity`).
   - Serviço de Processamento de Imagens e Miniaturas com SixLabors.ImageSharp (`PhotoService`).
5. [[05-camada-apresentacao|05. Camada de Apresentação (Presentation Layer)]]
   - Controllers MVC (`GameController`, `UserController`).
   - Endpoints de streaming de mídia (`GetPhoto`, `GetThumbnail`).
   - Views Razor responsivas com Bootstrap 5 e Bootstrap Icons.
   - Layout global e navegação integrada.

---

## 🏗️ Arquitetura e Visão Geral da Solução

O projeto adota uma **Arquitetura em Camadas (Layered Architecture)** com separação clara de responsabilidades:

```mermaid
graph TD
    A[Apresentação - Controllers & Views Razor] --> B[Aplicação - DTOs, Mappings, PhotoService]
    B --> C[Domínio - Entidades, Smart Enums, Interfaces]
    B --> D[Infraestrutura - AppDbContext, Repositórios, EF Core]
    D --> C
    D --> E[(Banco de Dados - Azure SQL / SQL Server)]
```

### 🛠️ Tecnologias e Bibliotecas Utilizadas
- **Linguagem & Framework:** .NET 10 / C# 13 (ASP.NET Core MVC)
- **ORM:** Entity Framework Core 10 (SQL Server Provider)
- **Banco de Dados:** Azure SQL Database / SQL Server Express
- **Processamento de Imagens:** `SixLabors.ImageSharp` (versão 3.x)
- **Interface Web:** Razor Views, Bootstrap 5.3, Bootstrap Icons 1.11

---

## 🚀 Guia Rápido de Reprodução da Aplicação

Caso precise recriar este projeto do zero em um novo ambiente:

### 1. Clonar / Inicializar o Projeto
```bash
dotnet new mvc -n fase-01 -f net10.0
cd fase-01
```

### 2. Instalar Pacotes NuGet
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package SixLabors.ImageSharp
```

### 3. Executar Migrations e Atualizar o Banco
Ganta que a connection string `DefaultConnection` está configurada no `appsettings.json`.
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Compilar e Rodar
```bash
dotnet build
dotnet run
```
Acesse `https://localhost:7216` (ou porta configurada) no seu navegador.
