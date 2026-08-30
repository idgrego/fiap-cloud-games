# 06. Roteiro de Testes Manuais via Scalar UI

#webapi #scalar #openapi #manual-testing #jwt #auth #governance #obsidian

Voltar para a [[index|Visão Geral]] | Ver anterior: [[05-camada-apresentacao|05. Camada de Apresentação]]

---

## 🎯 Objetivo

Este guia fornece um **roteiro de testes manuais, sequencial e prático**, projetado para validar todos os endpoints da **FIAP Cloud Games Web API** utilizando a interface interativa do **Scalar API Reference** (`/scalar/v1`).

O fluxo contempla a verificação de:
1. **Regra do Primeiro Administrador:** Atribuição automática de `Admin = true` ao primeiro usuário cadastrado na base.
2. **Ciclo de Autenticação JWT:** Emissão, captura e configuração do token Bearer no Scalar.
3. **Fotos em Formato Base64:** Envio de imagem codificada em Base64 via JSON (`[FromBody]`) nos endpoints de registro, criação e atualização (`PhotoBase64`).
4. **Exclusão de Fotos:** Endpoints específicos para remoção de foto de perfil (`DELETE /api/user/photo/{id}`) e foto de jogo (`DELETE /api/game/photo/{id}`).
5. **Controle de Acesso Granular (Roles e Claims):** Acesso individual restrito ao próprio usuário ou a administradores (`403 Forbidden` quando não autorizado).
6. **Governança de Administrador:** Impedimento de remoção do papel de Admin ou autoexclusão caso o usuário conectado seja o único administrador da aplicação (`400 BadRequest`).
7. **Catálogo de Jogos e Mídia:** CRUD completo e disparo de web scraping automático para fotos de jogos via URL.
8. **Middleware de Tratamento Global de Exceções:** Formatação padronizada para erros não capturados (`500 Internal Server Error`).

---

## 💡 Texto Base64 de Exemplo para Testes

Para requisições que aceitam foto no formato Base64 (`PhotoBase64`), você pode utilizar a seguinte string Data URI correspondente a uma imagem válida (PNG 1x1 em vermelho):

```text
data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==
```

---

## 🛠️ Pré-requisitos para Execução dos Testes

1. **Aplicação Web API em Execução:**
   ```bash
   dotnet watch --project fase-01/back-end/fase-01.csproj
   ```
2. **Interface do Scalar Acessível:**
   Navegue até 👉 **`http://localhost:5030/scalar/v1`** (ou porta configurada em `launchSettings.json`).
3. **Ambiente Limpo (Recomendado para o Passo 1):**
   Para validar a atribuição do primeiro administrador, garanta que a tabela `Users` esteja sem registros antes do início do fluxo.

---

## 🧪 Roteiro Sequencial de Testes Manuais

---

### 📍 Fase 1: Teste de Acesso Não Autenticado & Primeiro Cadastro (Administrador Automático)

#### Teste 1.1: Tentar Listar Usuários sem Autenticação
- **Endpoint:** `GET /api/user`
- **Autenticação no Scalar:** Nenhuma (campo Auth vazio)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `401 Unauthorized`
  - **Mensagem:** Requisição rejeitada por ausência de token de autorização.

#### Teste 1.2: Registro do Primeiríssimo Usuário do Sistema (com Foto em Base64 Opcional)
- **Endpoint:** `POST /api/account/register`
- **Content-Type:** `application/json`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "fullName": "Administrador Principal",
    "nickName": "admin_master",
    "email": "admin@fiapcloudgames.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "photoBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `201 Created`
  - **Corpo da Resposta:** Retorna o objeto criado.
  - **Efeito de Negócio:** Como o método `hasAnyUser()` indicava que a base estava vazia, este usuário recebe automaticamente `admin = true` e `validatedAt` preenchido com a data/hora atual. A foto enviada em `photoBase64` é processada, gerando o avatar e o thumbnail.

---

### 📍 Fase 2: Autenticação JWT e Configuração do Token no Scalar UI

#### Teste 2.1: Efetuar Login com a Conta Administradora
- **Endpoint:** `POST /api/account/login`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "email": "admin@fiapcloudgames.com",
    "password": "Password123!",
    "rememberMe": true
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `200 OK`
  - **Corpo da Resposta:**
    ```json
    {
      "token": "eyJhbGciOiJIUzI1Ni...",
      "user": {
        "id": 1,
        "fullName": "Administrador Principal",
        "email": "admin@fiapcloudgames.com",
        "nickName": "admin_master",
        "admin": true
      }
    }
    ```
  - **Cookie:** Define o cookie de sessão `jwt_token`.

#### Teste 2.2: Configurar o Bearer Token na Interface do Scalar
1. Copie o valor da propriedade `token` retornada no login (sem as aspas).
2. Na interface do Scalar, clique no botão **Auth** / **Authorization** (ou **Bearer Auth** no topo/lateral).
3. Cole o token no campo **Bearer Token**.
4. A partir deste momento, todas as requisições protegidas incluirão o cabeçalho `Authorization: Bearer <token>`.

---

### 📍 Fase 3: Registro de Usuário Comum, Manipulação de Foto e Autorização Granular

#### Teste 3.1: Cadastrar um Segundo Usuário (Usuário Comum)
- **Endpoint:** `POST /api/account/register`
- **Content-Type:** `application/json`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "fullName": "João Silva",
    "nickName": "joao_player",
    "email": "joao@email.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "photoBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `201 Created`
  - **Efeito de Negócio:** Como já existe outro usuário no banco, este novo cadastro é criado com `admin = false` (ID = 2).

#### Teste 3.2: Autenticar como Usuário Comum
- **Endpoint:** `POST /api/account/login` com as credenciais de `joao@email.com`.
- **Ação:** Copie o novo token e substitua o token ativo no Scalar pelo token de **João Silva** (ID = 2, `admin = false`).

#### Teste 3.3: Usuário Comum Consultando Seu Próprio Perfil e Foto
- **`GET /api/user/2`**: Retorna dados cadastrais (`200 OK`).
- **`GET /api/user/photo/2`**: Retorna a imagem inteira (`200 OK`, `Content-Type: image/png`).
- **`GET /api/user/thumbnail/2`**: Retorna a miniatura redimensionada (`200 OK`, `Content-Type: image/jpeg`).

#### Teste 3.4: Exclusão da Própria Foto pelo Usuário
- **Endpoint:** `DELETE /api/user/photo/2`
- **Autenticação Ativa:** Token do usuário João (ID 2)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `204 No Content`
  - **Efeito:** A foto do usuário 2 é excluída do banco.
  - **Validação Secundária:** Executar `GET /api/user/photo/2` agora deve retornar `404 Not Found`.

#### Teste 3.5: Usuário Comum Tentando Consultar Perfil de Terceiros (Negado)
- **Endpoint:** `GET /api/user/1` (Tentando ver dados do Admin)
- **Autenticação Ativa:** Token do usuário João (ID 2)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `403 Forbidden`
  - **Regra de Negócio:** Usuários sem papel `Admin` só possuem permissão para visualizar o seu próprio perfil.

#### Teste 3.6: Usuário Comum Tentando Excluir Foto de Outro Usuário (Negado)
- **Endpoint:** `DELETE /api/user/photo/1` (Tentando excluir a foto do Admin)
- **Autenticação Ativa:** Token do usuário João (ID 2)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `400 Bad Request` ou `403 Forbidden` com mensagem de erro informando que apenas o próprio usuário ou um admin pode excluir a foto.

#### Teste 3.7: Usuário Comum Tentando Listar Todos os Usuários (Negado)
- **Endpoint:** `GET /api/user`
- **Autenticação Ativa:** Token do usuário João (ID 2)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `403 Forbidden` (`[Authorize(Roles = "Admin")]`).

---

### 📍 Fase 4: Governança, Alteração com Foto Base64 & Segurança do Último Admin

> **Configuração:** Troque o token ativo no Scalar de volta para o token do **Administrador Principal** (ID = 1, `admin = true`).

#### Teste 4.1: Atualização de Usuário via JSON com Foto Base64
- **Endpoint:** `PUT /api/user/2`
- **Content-Type:** `application/json`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "id": 2,
    "fullName": "João Silva Atualizado",
    "nickName": "joao_v2",
    "email": "joao@email.com",
    "admin": false,
    "photoBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `204 No Content`
  - **Efeito:** Os dados cadastrais e a foto de perfil em Base64 são atualizados com sucesso.

#### Teste 4.2: Tentar Remover Permissão de Admin do Único Administrador (Bloqueado)
- **Endpoint:** `PUT /api/user/1`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "id": 1,
    "fullName": "Administrador Principal",
    "nickName": "admin_master",
    "email": "admin@fiapcloudgames.com",
    "admin": false
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `400 Bad Request`
  - **Resposta de Validação:** `Não é possível remover a permissão de administrador do seu próprio usuário enquanto você for o único administrador do sistema.`

#### Teste 4.3: Tentar Autoexcluir a Conta do Único Administrador (Bloqueado)
- **Endpoint:** `DELETE /api/user/1`
- **Autenticação Ativa:** Token do Admin (ID 1)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `400 Bad Request`
  - **Resposta de Validação:** `Não é possível excluir o seu usuário enquanto você for o único administrador do sistema.`

#### Teste 4.4: Promover o Segundo Usuário a Administrador
- **Endpoint:** `PUT /api/user/2`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "id": 2,
    "fullName": "João Silva Admin",
    "nickName": "joao_admin",
    "email": "joao@email.com",
    "admin": true
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `204 No Content`
  - **Efeito:** Agora existem dois administradores no banco de dados (ID 1 e ID 2).

#### Teste 4.5: Excluir a Conta do Primeiro Administrador com Sucesso (Liberado)
- **Endpoint:** `DELETE /api/user/1`
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `204 No Content`
  - **Efeito:** A operação é permitida pois o sistema mantém o usuário João Silva (ID 2) como administrador ativo.

---

### 📍 Fase 5: Gerenciamento do Catálogo de Jogos, Fotos Base64 & Scraping Automático

#### Teste 5.1: Listagem Pública de Jogos
- **Endpoint:** `GET /api/game`
- **Autenticação:** Nenhuma necessária (endpoint público)
- **Ação:** Clique em **Send**.
- **Resultado Esperado:** `200 OK` com a lista de jogos cadastrados.

#### Teste 5.2: Cadastrar um Novo Jogo com Foto em Base64 (JSON)
- **Endpoint:** `POST /api/game`
- **Content-Type:** `application/json`
- **Autenticação Ativa:** Token do Administrador
- **Corpo da Requisição (JSON):**
  ```json
  {
    "name": "God of War Ragnarök",
    "manufacturer": "Sony Interactive Entertainment",
    "categoryId": 1,
    "description": "Jornada épica de Kratos e Atreus pelos nove reinos.",
    "online": false,
    "multiplayer": false,
    "urlGame": "https://store.playstation.com/pt-br/product/UP9000-CUSA34388_00-GOWRAGNAROK00000",
    "urlVideo": "https://www.youtube.com/watch?v=hfJ4Km46A-0",
    "photoBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `201 Created`
  - **Header `Location`:** `/api/Game/{id}`

#### Teste 5.3: Consultar Foto e Miniatura do Jogo
- **`GET /api/game/photo/{id}`**: Retorna a imagem inteira do jogo (`200 OK`).
- **`GET /api/game/thumbnail/{id}`**: Retorna a miniatura do jogo (`200 OK`).

#### Teste 5.4: Atualizar Foto do Jogo via JSON Base64
- **Endpoint:** `PUT /api/game/{id}`
- **Content-Type:** `application/json`
- **Corpo da Requisição (JSON):**
  ```json
  {
    "id": 1,
    "name": "God of War Ragnarök - Remastered",
    "manufacturer": "Sony Interactive Entertainment",
    "categoryId": 1,
    "description": "Edição com fotos atualizadas.",
    "online": false,
    "multiplayer": false,
    "photoBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
  }
  ```
- **Ação:** Clique em **Send**.
- **Resultado Esperado:** `204 No Content`.

#### Teste 5.5: Excluir Apenas a Foto do Jogo
- **Endpoint:** `DELETE /api/game/photo/{id}`
- **Autenticação Ativa:** Token de Administrador
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `204 No Content`
  - **Validação:** Requisições posteriores a `GET /api/game/photo/{id}` retornarão `404 Not Found`.

#### Teste 5.6: Teste de Web Scraping Automático de Capa
1. Cadastre um jogo enviando `urlGame` válida, mas **sem enviar o campo `photoBase64`**.
2. Execute `GET /api/game/{id_do_novo_jogo}`.
3. **Resultado Esperado:**
   - A API detecta a ausência da imagem e a presença de `urlGame`.
   - Dispara o serviço `PhotoService.ScrapGameImageAsync`, faz download automático da imagem de capa da loja externa, redimensiona o thumbnail e salva no banco.
   - Retorna `200 OK`.

#### Teste 5.7: Exclusão de Jogo (com Exclusão em Cascata da Foto)
- **`DELETE /api/game/{id}`**: Exclui o jogo e dispara a remoção da foto associada por *Cascade Delete* (`204 No Content`).

---

### 📍 Fase 6: Teste de Exceção Não Tratada e Encerramento de Sessão

#### Teste 6.1: Disparo do Middleware de Exceções
- **Endpoint:** `GET /api/account/test-error`
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `500 Internal Server Error`
  - **Estrutura JSON Formatada (`ExceptionHandlingMiddleware`):**
    ```json
    {
      "status": 500,
      "error": "Internal Server Error",
      "message": "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
      "path": "/api/account/test-error",
      "timestamp": "2026-08-29T..."
    }
    ```

#### Teste 6.2: Encerramento de Sessão (Logout)
- **Endpoint:** `POST /api/account/logout`
- **Autenticação Ativa:** Bearer Token
- **Ação:** Clique em **Send**.
- **Resultado Esperado:**
  - **Status Code:** `200 OK`
  - **Mensagem:** `{"message": "Logout realizado com sucesso."}`
  - **Header `Set-Cookie`:** `jwt_token=; expires=Thu, 01 Jan 1970 00:00:00 GMT`

---

## 📋 Tabela Resumo do Roteiro de Testes

| Etapa | Endpoint | Método | Papel Mínimo | Payload / Params | Status Esperado | Validação Principal |
|---|---|---|---|---|---|---|
| **1.1** | `/api/user` | `GET` | Não Autenticado | NENHUM | `401 Unauthorized` | Rejeição de requisição sem token |
| **1.2** | `/api/account/register` | `POST` | Público | JSON (`RegisterDto` com `photoBase64`) | `201 Created` | 1º usuário vira Admin automático (`Admin = true`) com foto Base64 |
| **2.1** | `/api/account/login` | `POST` | Público | JSON (`LoginDto`) | `200 OK` | Emissão de JWT token e cookie `jwt_token` |
| **3.1** | `/api/account/register` | `POST` | Público | JSON (`RegisterDto` com `photoBase64`) | `201 Created` | 2º usuário criado como Usuário Comum (`Admin = false`) |
| **3.3** | `/api/user/2` | `GET` | Autenticado | Route `id=2` | `200 OK` | Consulta do próprio perfil e mídias (`photo`/`thumbnail`) |
| **3.4** | `/api/user/photo/2` | `DELETE` | Autenticado | Route `id=2` | `204 No Content` | Exclusão da própria foto pelo usuário comum |
| **3.5** | `/api/user/1` | `GET` | Autenticado | Route `id=1` | `403 Forbidden` | Bloqueio de acesso a perfil alheio por cliente comum |
| **3.6** | `/api/user/photo/1` | `DELETE` | Autenticado | Route `id=1` | `400 Bad Request` / `403` | Bloqueio de exclusão da foto de terceiros |
| **3.7** | `/api/user` | `GET` | Autenticado | NENHUM | `403 Forbidden` | Bloqueio de listagem completa para cliente comum |
| **4.1** | `/api/user/2` | `PUT` | Admin | JSON (`UserDto` com `photoBase64`) | `204 No Content` | Atualização de perfil com foto em Base64 |
| **4.2** | `/api/user/1` | `PUT` | Admin (Único) | JSON (`admin = false`) | `400 Bad Request` | Bloqueio de remoção do status de Admin do único admin |
| **4.3** | `/api/user/1` | `DELETE` | Admin (Único) | Route `id=1` | `400 Bad Request` | Bloqueio de autoexclusão do único administrador |
| **4.4** | `/api/user/2` | `PUT` | Admin | JSON (`admin = true`) | `204 No Content` | Promoção de 2º usuário a Administrador |
| **4.5** | `/api/user/1` | `DELETE` | Admin | Route `id=1` | `204 No Content` | Exclusão do 1º admin permitida pois há outro admin ativo |
| **5.2** | `/api/game` | `POST` | Admin | JSON (`GameDto` com `photoBase64`) | `201 Created` | Cadastro de jogo com capa em Base64 |
| **5.4** | `/api/game/{id}` | `PUT` | Admin | JSON (`GameDto` com `photoBase64`) | `204 No Content` | Atualização do jogo e da foto em Base64 |
| **5.5** | `/api/game/photo/{id}` | `DELETE` | Admin | Route `id` | `204 No Content` | Exclusão individual da foto do jogo |
| **5.6** | `/api/game/{id}` | `GET` | Público | Route `id` | `200 OK` | Disparo de Web Scraping automático para games sem foto |
| **6.1** | `/api/account/test-error` | `GET` | Público | NENHUM | `500 Internal Server Error` | Formatação padronizada do Middleware de Exceção |
| **6.2** | `/api/account/logout` | `POST` | Autenticado | NENHUM | `200 OK` | Revogação do cookie de sessão `jwt_token` |

---

Voltar para a [[index|Visão Geral]] | Ver anterior: [[05-camada-apresentacao|05. Camada de Apresentação]]
