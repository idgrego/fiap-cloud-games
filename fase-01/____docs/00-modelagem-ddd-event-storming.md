# 00. Modelagem DDD (Event Storming & Diagrama de Contexto)

#ddd #event-storming #context-map #domain #architecture #obsidian #mermaid

Voltar para a [Visão Geral](index.md) | Ver próximo: [01. Scripts de Banco de Dados](01-scripts-banco-dados.md)

---

## 🎯 Objetivo da Modelagem DDD

No contexto do **Domain-Driven Design (DDD)**, o entendimento do domínio e das regras de negócio precede a implementação técnica e o modelo de persistência. 

Neste módulo, estão documentados os artefatos de modelagem da **Fase 1**:
1. **Event Storming**: Mapeamento comportamental e temporal dos eventos de domínio, comandos, agregados e atores para os fluxos de **Usuários** e **Jogos**.
2. **Diagrama de Contexto (Context Map)**: Mapeamento dos **Bounded Contexts** (Contextos Delimitados) da aplicação e os relacionamentos entre eles.

Toda a modelagem foi construída utilizando sintaxe **Mermaid**, permitindo navegação, visualização interativa e suporte nativo no **Obsidian** e **GitHub**.

---

## 🟧 Event Storming dos Fluxos de Negócio

No Event Storming, utilizamos a convenção visual de cores padronizada do DDD:

- 👤 **Actor / Usuário** (Cinza - `#eceff1`): Papel humano ou sistema cliente que dispara uma intenção.
- 🟦 **Command** (Azul - `#bbdefb`): Ação ou intenção solicitada ao sistema.
- 🟨 **Aggregate / Entidade** (Amarelo - `#fff9c4`): Unidade de regra de negócio e consistência de dados.
- 🟧 **Domain Event** (Laranja - `#ffe0b2`): Ocorrência relevante no passado do domínio (verbo no particípio/passado).

---

### 1. Event Storming: Fluxo de Criação e Autenticação de Usuários

Este fluxo abrange desde o registro de um novo usuário visitante até o hash de senhas, criação de conta e geração do token JWT.

```mermaid
flowchart LR
    %% Definição de Cores do Event Storming
    classDef actor fill:#eceff1,stroke:#455a64,color:#102a43,stroke-width:2px;
    classDef command fill:#bbdefb,stroke:#1e88e5,color:#0d47a1,stroke-width:2px;
    classDef aggregate fill:#fff9c4,stroke:#fbc02d,color:#f57f17,stroke-width:2px;
    classDef event fill:#ffe0b2,stroke:#fb8c00,color:#e65100,stroke-width:2px;

    %% Atores
    Visitor[👤 Visitante]:::actor
    UserSystem[👤 Usuário Cadastrado]:::actor

    %% Sub-fluxo 1: Registro
    Visitor --> C_Register["[Comando] Registrar usuário"]:::command
    C_Register --> A_User["[Agregação] Usuário / Conta"]:::aggregate
    A_User --> E_UserCreated["[Evento] Usuário registrado"]:::event
    E_UserCreated --> C_Hash["[Comando] Gerar hash da senha"]:::command
    C_Hash --> E_PasswordHashed["[Evento] Hash gerado e salvo (Conta)"]:::event

    %% Sub-fluxo 2: Login e Autenticação
    UserSystem --> C_Login["[Comando] Autenticar"]:::command
    C_Login --> A_Account["[Agregação] Usuário / Conta"]:::aggregate
    A_Account --> E_Auth["[Evento] Usuário autenticado"]:::event
    E_Auth --> C_Jwt["[Comando] Token JWT gerado"]:::command
    C_Jwt --> E_TokenIssued["[Evento] Token JWT emitido"]:::event
```

#### 📌 Detalhamento dos Componentes do Fluxo:
- **Comandos**: `Registrar usuário` (Solicita cadastro com nome, nickname, e-mail e senha), `Autenticar` (Solicita login com e-mail e senha).
- **Agregados**: `Usuário` e `Conta` garantem, respectivamente, a unicidade do e-mail com formato válido e integridade do hash PBKDF2/HMAC-SHA512.
- **Eventos de Domínio**: `Usuário registrado`, `Hash gerado e salvo (Conta)`, `Usuário autenticado`, `Token JWT emitido`.

---

### 2. Event Storming: Fluxo de Criação e Gerenciamento de Jogos

Este fluxo contempla o cadastro de jogos no catálogo por administradores, validação de categorias (*Smart Enum*) e upload/redimensionamento da foto de capa.

```mermaid
flowchart LR
    %% Definição de Cores do Event Storming
    classDef actor fill:#eceff1,stroke:#455a64,color:#102a43,stroke-width:2px;
    classDef command fill:#bbdefb,stroke:#1e88e5,color:#0d47a1,stroke-width:2px;
    classDef aggregate fill:#fff9c4,stroke:#fbc02d,color:#f57f17,stroke-width:2px;
    classDef event fill:#ffe0b2,stroke:#fb8c00,color:#e65100,stroke-width:2px;

    %% Atores
    Admin[👤 Administrador]:::actor

    %% Fluxo de Cadastro e Mídia de Jogos
    Admin --> C_CreateGame["[Comando] Criar jogo"]:::command
    C_CreateGame --> A_Game["[Agregado] Jogo / Categoria"]:::aggregate
    A_Game --> E_GameCreated["[Evento] Jogo criado"]:::event

    E_GameCreated --> C_UploadPhoto["[Comando] Upload da foto do jogo"]:::command
    C_UploadPhoto --> A_Photo["[Agregado] Foto do jogo / PhotoService"]:::aggregate
    A_Photo --> E_PhotoProcessed["[Evento] Thumbnail gerado e foto salva"]:::event
```

#### 📌 Detalhamento dos Componentes do Fluxo:
- **Comandos**: `Criar jogo` (Informa título, fabricante, descrição, modo online/multiplayer e categoria), `Upload da foto do jogo` (Envia arquivo de imagem de capa).
- **Agregados**: `Jogo` (valida obrigatoriedade de campos e conversão da categoria) e `Foto do jogo` (processa e armazena imagem e miniatura).
- **Eventos de Domínio**: `Jogo criado`, `Thumbnail gerado e foto salva`.

---

## 🗺️ Diagrama de Contexto (Context Map)

O **Mapa de Contexto** estabelece os limites explícitos de cada **Bounded Context** (Contexto Delimitado) da solução e define a forma como eles se comunicam.

```mermaid
graph TD
    subgraph Boundary_Auth ["🔐 Bounded Context: Autenticação e Identidade"]
        direction TB
        UserAgg[Agregação do usuário e conta]
        JwtSvc[Serviço JWT: gera o token, valida e extrai os dados]
        PassSvc[Serviço de senha: gera o hash e valida a senha]
    end

    subgraph Boundary_Catalog ["🎮 Bounded Context: Catálogo de Jogos"]
        direction TB
        GameAgg[Agregação do jogo, categoria e foto do jogo]
        PhotoSvc["Serviço de foto: gera o thumbnail (miniatura)"]
    end

    subgraph Infrastructure ["☁️ Infraestrutura e Persistência"]
        AppDb[(Contexto para o Azure SQL Database)]
    end

    %% Relacionamentos de Contexto (Envio / Recebimento)
    Boundary_Auth -->|"Fornece Tokens e Claims de Autorização (Envia)"| Boundary_Catalog
    Boundary_Catalog -->|"Consome Identidade e Validação de Role Admin (Recebe)"| Boundary_Auth

    Boundary_Auth --> AppDb
    Boundary_Catalog --> AppDb
```

### 📖 Padrões de Relacionamento Entre os Contextos:

1. **Contexto de Autenticação e Identidade (`Boundary_Auth`)**:
   - Localiza o usuário pelo e-mail e valida a senha com o hash armazenado.
   - Fornece o token JWT e gerencia permissões e roles (`Admin` / `User`).
   - Responsável pelo ciclo de vida do usuário, segurança de credenciais e emissão das *claims* (ex: `NameIdentifier` = ID do usuário).
   - **Regras de Governança de Usuários:**
     - *Consulta Individual:* Acesso restrito ao próprio usuário autenticado ou a administradores.
     - *Proteção de Administrador:* Impede a desativação da permissão de admin ou autoexclusão caso o usuário conectado seja o **único administrador** cadastrado no sistema.

2. **Contexto de Catálogo de Jogos (`Boundary_Catalog`)**:
   - Fornece dados e as respectivas fotos dos jogos cadastrados.
   - Garante proteção para manipulação dos dados através do JWT. Este contém o ID do usuário e assim é possível identificar se é ou não um administrador do sistema.
   - Apenas administradores podem realizar manipulação dos dados e da foto.

3. **Camada de Persistência Compartilhada (`Infrastructure`)**:
   - Mapeada via Entity Framework Core, isolando os modelos relacionais das regras de negócio ricas do domínio.

---

Voltar para a [Visão Geral](index.md) | Ver próximo: [01. Scripts de Banco de Dados](01-scripts-banco-dados.md)

