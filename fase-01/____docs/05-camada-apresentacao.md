# 05. Camada de Apresentação (Presentation Layer - Web API)

#aspnetcore #webapi #rest #jwt #openapi #scalar #controllers #csharp

Voltar para a [[index|Visão Geral]] | Ver anterior: [[04-camada-aplicacao|04. Camada de Aplicação]] | Ver próxima: [[06-roteiro-testes-scalar|06. Roteio de testes usando Scalar]]

---

## 🎯 Objetivo

A camada de **Apresentação** é a interface HTTP do sistema. Trata-se de aplicação **Web API RESTful**, pronta para ser consumida por qualquer cliente front-end (React, Angular, Vue, Mobile ou aplicações desktop).

A API expõe dados e ações no formato **JSON**, gerencia autenticação e autorização via **JWT Bearer Tokens** e fornece documentação interativa moderna através de **OpenAPI Nativo (`Microsoft.AspNetCore.OpenApi`)** e **Scalar UI (`Scalar.AspNetCore`)**.

---

## 📂 Estrutura da Camada de Apresentação

```
Controllers/
├── AccountController.cs
├── GameController.cs
└── UserController.cs
Properties/
└── launchSettings.json
Program.cs
```

---

## 🛡️ Autenticação e Autorização via JWT

A segurança da API é construída sobre o middleware `Microsoft.AspNetCore.Authentication.JwtBearer`:

1. **Validação Estrita de Assinatura e Expiração:** O middleware valida a `SecretKey`, `Issuer`, `Audience` e expiração do token sem tolerância de tempo extra (`ClockSkew = TimeSpan.Zero`).
2. **Re-validação no Banco de Dados (`OnTokenValidated`):** A cada requisição autenticada, o ID do usuário extraído das claims do token é consultado no repositório. Isso garante que bloqueios de conta ou alterações de permissões entrem em vigor **imediatamente**.
3. **Suporte Duplo (Header ou Cookie):** O evento `OnMessageReceived` permite capturar o token enviado no cabeçalho `Authorization: Bearer <token>` ou no cookie HTTP-Only `jwt_token`.

---

## 🎮 Controllers Web API (`[ApiController]`)

### 1. `AccountController.cs` (Registro, Login e Logout)
Endpoint base: `/api/account`

```csharp
namespace fase_01.Controllers;

using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.domain.entities;
using fase_01.domain.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettingsDto _jwtSettings;

    public AccountController(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettingsDto> jwtSettings)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Cadastra um novo usuário e cria a conta associada com hash de senha.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Este e-mail já está em uso por outro usuário." });

        var passwordHash = _passwordService.HashPassword(dto.Password);

        var user = new User
        {
            FullName = dto.FullName,
            NickName = dto.NickName,
            Email = dto.Email,
            Admin = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateWithAccountAsync(user, passwordHash);

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.FullName, user.Email });
    }

    /// <summary>
    /// Autentica o usuário, valida o hash da senha e gera o token JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || user.Account == null || !_passwordService.VerifyPassword(dto.Password, user.Account.PasswordHash))
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        var token = _jwtTokenService.GenerateToken(user, dto.RememberMe);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = dto.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
                : DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
        };
        Response.Cookies.Append("jwt_token", token, cookieOptions);

        return Ok(new
        {
            token,
            user = new { user.Id, user.FullName, user.Email, user.NickName, user.Admin }
        });
    }

    /// <summary>
    /// Encerra a sessão removendo o cookie da aplicação.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt_token");
        return Ok(new { message = "Logout realizado com sucesso." });
    }
}
```

---

### 2. `GameController.cs` (CRUD de Jogos, Fotos e Web Scraping Automático de Mídia)
Endpoint base: `/api/game`

- **Listagem Pública:** `GET /api/game`
- **Consulta por ID:** `GET /api/game/{id}` (realiza scraping automático da imagem se `UrlGame` estiver preenchida e a foto ainda não existir)
- **Criação / Atualização:** `POST /api/game`, `PUT /api/game/{id}` (`[Authorize(Roles = "Admin")]`, com fotos em formato Base64 via `[FromBody]`)
- **Exclusão de Jogo:** `DELETE /api/game/{id}` (`[Authorize(Roles = "Admin")]`)
- **Exclusão de Foto do Jogo:** `DELETE /api/game/photo/{id}` (`[Authorize(Roles = "Admin")]`)

```csharp
namespace fase_01.Controllers;

using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.mappings;
using fase_01.application.dtos;
using fase_01.application.services;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPhotoService _photoService;

    public GameController(IGameRepository gameRepository, IPhotoService photoService)
    {
        _gameRepository = gameRepository;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _gameRepository.ListAllAsync();
        var dtos = entities.Select(g => g.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _gameRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        if (entity.Photo == null && !string.IsNullOrWhiteSpace(entity.UrlGame))
        {
            var newGamePhoto = await PhotoService.ScrapGameImageAsync(id, entity.UrlGame);
            if (newGamePhoto != null) await _photoService.SaveGamePhotoAsync(newGamePhoto);
        }

        return Ok(entity.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] GameDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = dto.ToEntity();
        await _gameRepository.AddAsync(entity);
        await _photoService.SaveGamePhotoAsync(entity.Id, dto.PhotoBase64);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] GameDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _gameRepository.GetByIdAsync(dto.Id);
        if (existingEntity == null) return NotFound();

        var entity = dto.ToEntity(existingEntity);
        await _gameRepository.UpdateAsync(entity);
        await _photoService.SaveGamePhotoAsync(entity.Id, dto.PhotoBase64);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _gameRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpDelete("photo/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        await _photoService.DeleteGamePhotoAsync(id);
        return NoContent();
    }
}
```

---

### 3. `UserController.cs` (Gerenciamento de Usuários e Fotos)
Endpoint base: `/api/user`

#### 🛡️ Regras de Segurança e Governança
- **Listagem Geral (`GET /api/user`):** Restrito a usuários com a role `Admin` (`[Authorize(Roles = "Admin")]`).
- **Detalhes do Usuário (`GET /api/user/{id}`):** Requer autenticação (`[Authorize]`). O endpoint valida se o `id` solicitado pertence ao próprio usuário conectado (extraído da claim `NameIdentifier`) ou se quem está consultando é um administrador (`User.IsInRole("Admin")`). Se um usuário comum tentar visualizar os dados de outro usuário, a API retorna `403 Forbidden`.
- **Atualização (`PUT /api/user/{id}`):** Restrito a administradores (`[Authorize(Roles = "Admin")]`). Recebe os dados via `[FromBody] UserDto` em JSON, incluindo foto opcional em Base64 (`PhotoBase64`). Inclui regra de proteção para evitar a desativação acidental do único administrador.
- **Exclusão de Usuário (`DELETE /api/user/{id}`):** Restrito a administradores (`[Authorize(Roles = "Admin")]`). Protege o sistema contra autoexclusão do último administrador.
- **Exclusão de Foto de Perfil (`DELETE /api/user/photo/{id}`):** Requer autenticação (`[Authorize]`). Permite que o próprio usuário ou um administrador exclua a foto de perfil.
- **Mídia / Fotos (`GET /api/user/photo/{id}`, `GET /api/user/thumbnail/{id}`):** Endpoints públicos para obtenção da foto de perfil e sua miniatura redimensionada.

```csharp
namespace fase_01.Controllers;

using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.dtos;
using fase_01.application.mappings;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;

    public UserController(IUserRepository userRepository, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _photoService = photoService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _userRepository.ListAllAsync();
        var dtos = entities.Select(u => u.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserIdClaims = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserIdClaims, out var currentUserId))
            return Forbid();
        if (!(currentUserId == id || User.IsInRole("Admin")))
            return Forbid();

        var entity = await _userRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        return Ok(entity.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _userRepository.GetByIdAsync(dto.Id);
        if (existingEntity == null) return NotFound();

        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(currentUserIdClaim, out var currentUserId);

        // Se o usuário logado estiver tentando remover o seu próprio status de Admin
        if (currentUserId == id && existingEntity.Admin && !dto.Admin)
        {
            var allUsers = await _userRepository.ListAllAsync();
            bool hasOtherAdmin = allUsers.Any(u => u.Admin && u.Id != id);

            if (!hasOtherAdmin)
            {
                ModelState.AddModelError("Admin", "Não é possível remover a permissão de administrador do seu próprio usuário enquanto você for o único administrador do sistema.");
                return BadRequest(ModelState);
            }
        }

        var entity = dto.ToEntity(existingEntity);
        await _userRepository.UpdateAsync(entity);
        await _photoService.SaveUserPhotoAsync(entity.Id, dto.PhotoBase64);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var existingEntity = await _userRepository.GetByIdAsync(id);
        if (existingEntity == null) return NoContent();

        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(currentUserIdClaim, out var currentUserId);

        if (currentUserId == id && existingEntity.Admin)
        {
            var allUsers = await _userRepository.ListAllAsync();
            bool hasOtherAdmin = allUsers.Any(u => u.Admin && u.Id != id);

            if (!hasOtherAdmin)
            {
                ModelState.AddModelError("Admin", "Não é possível excluir o seu usuário enquanto você for o único administrador do sistema.");
                return BadRequest(ModelState);
            }
        }

        await _userRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("photo/{id:int}")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _photoService.GetUserPhotoAsync(id);
        if (photo == null || photo.Image == null || photo.Image.Length == 0)
            return NotFound();

        return File(photo.Image, photo.ContentType);
    }

    [HttpGet("thumbnail/{id:int}")]
    public async Task<IActionResult> GetThumbnail(int id)
    {
        var photo = await _photoService.GetUserPhotoAsync(id);
        if (photo == null || (photo.Thumbnail == null && photo.Image == null))
            return NotFound();

        var bytes = photo.Thumbnail ?? photo.Image;
        return File(bytes, "image/jpeg");
    }

    [HttpDelete("photo/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var existingEntity = await _userRepository.GetByIdAsync(id);
        if (existingEntity == null) return NoContent();

        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(currentUserIdClaim, out var currentUserId);

        if (!(currentUserId == id || existingEntity.Admin))
        {
            ModelState.AddModelError("DefaultErrorMessage", "Apenas o próprio usuário ou um administrador pode excluir a foto");
            return BadRequest(ModelState);
        }

        await _photoService.DeleteUserPhotoAsync(id);
        return NoContent();
    }
}
```

---

## 📖 Documentação Interativa com OpenAPI Nativo & Scalar

Substituindo o antigo Swashbuckle, o projeto adota as bibliotecas oficiais do .NET 10:
- **`Microsoft.AspNetCore.OpenApi`**: Gera a especificação OpenAPI JSON em `/openapi/v1.json`.
- **`Scalar.AspNetCore`**: Renderiza a interface de teste interativa moderna em `/scalar/v1`.

### Trecho de Configuração no `Program.cs`:
```csharp
// 1. Registrar OpenAPI com suporte a JWT Security Scheme
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "FIAP Cloud Games API",
            Version = "v1",
            Description = "API RESTful para gerenciamento de jogos e usuários"
        };

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT gerado no endpoint de login"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes?.Add("Bearer", scheme);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// 2. Habilitar o Endpoint OpenAPI e a UI do Scalar em Desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "FIAP Cloud Games API - Documentação";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
    });
}
```

---

## 🚀 Como Executar e Testar a API

1. Execute o projeto no terminal:
   ```bash
   dotnet watch
   ```
2. O navegador abrirá automaticamente na interface interativa do Scalar:
   👉 **`http://localhost:5030/scalar/v1`**
3. Realize a chamada `POST /api/account/login`, copie a string `token` da resposta e cole na seção **Bearer Auth** do Scalar para autenticar requisições protegidas!

@using fase_01.Models
@using fase_01.application.dtos
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

### 2. Layout Global (`_Layout.cshtml`)
Fornece um cabeçalho com tema escuro (*dark navbar*), marca customizada **FIAP Cloud Games**, ícones do Bootstrap e menu responsivo:

```razor
<!DOCTYPE html>
<html lang="pt-br">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - FIAP Cloud Games</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-dark bg-dark border-bottom box-shadow mb-3">
            <div class="container-fluid">
                <a class="navbar-brand text-primary fw-bold" asp-controller="Home" asp-action="Index">
                    <i class="bi bi-cloud-fill me-1"></i> FIAP Cloud Games
                </a>
                <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link text-light" asp-controller="Home" asp-action="Index"><i class="bi bi-house-door-fill me-1"></i>Início</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link text-light" asp-controller="Game" asp-action="Index"><i class="bi bi-controller me-1"></i>Jogos</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link text-light" asp-controller="User" asp-action="Index"><i class="bi bi-people-fill me-1"></i>Usuários</a>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    <div class="container">
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>
    <footer class="border-top footer text-muted">
        <div class="container">
            &copy; 2026 - FIAP Cloud Games
        </div>
    </footer>
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### 3. Exibição Dinâmica de Imagens no HTML
Nas views Razor, a tag `<img>` aponta diretamente para as Actions do Controller responsável por retornar o array de bytes da imagem:

```razor
<!-- Exemplo na View de Catálogo de Jogos (Index.cshtml) -->
<img src="@Url.Action("GetThumbnail", "Game", new { id = item.Id })" 
     alt="@item.Name" 
     class="img-fluid rounded my-auto" 
     onerror="this.onerror=null; this.src='https://via.placeholder.com/300x180?text=Sem+Foto';" />
```

> [!TIP] Tratamento de Imagens Ausentes (`onerror`)
> O evento JavaScript `onerror` é configurado nas imagens para exibir um *placeholder* padrão automaticamente caso o registro no banco não possua foto cadastrada.

---

## 💉 Injeção de Dependências (`Program.cs`)

Para unir todas as camadas e permitir que os Controllers recebam as instâncias dos Serviços e Repositórios, os serviços foram registrados no container do ASP.NET Core no `Program.cs`:

```csharp
// Registro dos Repositórios
builder.Services.AddScoped<IGamePhotoRepository, GamePhotoRepository>();
builder.Services.AddScoped<IUserPhotoRepository, UserPhotoRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Registro do Serviço de Aplicação
builder.Services.AddScoped<IPhotoService, PhotoService>();

// Suporte ao MVC
builder.Services.AddControllersWithViews();
```

---

Voltar para a [[index|Visão Geral]] | Ver anterior: [[04-camada-aplicacao|04. Camada de Aplicação]] | Ver próxima: [[06-roteiro-testes-scalar|06. Roteio de testes usando Scalar]]
