# 05. Camada de Apresentação (Presentation Layer)

#aspnetcore #mvc #razor #bootstrap #controllers #views #csharp

Voltar para a [[index|Visão Geral]] | Ver anterior: [[04-camada-aplicacao|04. Camada de Aplicação]]

---

## 🎯 Objetivo

A camada de **Apresentação** é a interface do usuário com a aplicação web. Ela é construída com o padrão **ASP.NET Core MVC (Model-View-Controller)**, utilizando Views Razor renderizadas no servidor e estilizadas com **Bootstrap 5.3** e **Bootstrap Icons**.

---

## 📂 Estrutura da Camada de Apresentação

```
Controllers/
├── GameController.cs
├── HomeController.cs
└── UserController.cs

Views/
├── _ViewImports.cshtml
├── _ViewStart.cshtml
├── Game/
│   ├── Create.cshtml
│   ├── Delete.cshtml
│   ├── Detail.cshtml
│   ├── Edit.cshtml
│   └── Index.cshtml
├── Home/
│   ├── Index.cshtml
│   └── Privacy.cshtml
├── Shared/
│   ├── _Layout.cshtml
│   ├── _Layout.cshtml.css
│   ├── _ValidationScriptsPartial.cshtml
│   └── Error.cshtml
└── User/
    ├── Create.cshtml
    ├── Delete.cshtml
    ├── Detail.cshtml
    ├── Edit.cshtml
    └── Index.cshtml
```

---

## 🕹️ Controllers MVC

### 1. `GameController.cs` e Endpoints de Mídia
O `GameController` gerencia todo o ciclo de vida de cadastro, listagem, edição e exclusão de jogos, além de expor endpoints que servem os arquivos de imagem/miniatura gravados em formato binário no banco de dados.

```csharp
using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.mappings;
using fase_01.domain.enums;
using fase_01.application.dtos;

namespace fase_01.Controllers;

public class GameController : Controller
{
    private readonly IGameRepository _gameRepository;
    private readonly IPhotoService _photoService;

    public GameController(IGameRepository gameRepository, IPhotoService photoService)
    {
        _gameRepository = gameRepository;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var entities = await _gameRepository.ListAllAsync();
        var dtos = entities.Select(g => g.ToDto());
        return View(dtos);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var entity = await _gameRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity.ToDto());
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = GameCategory.List();
        return View(new GameDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GameDto dto)
    {
        if (ModelState.IsValid)
        {
            var entity = dto.ToEntity();
            await _gameRepository.AddAsync(entity);

            if (dto.Photo != null && dto.Photo.Length > 0)
                await _photoService.SaveGamePhotoAsync(entity.Id, dto.Photo);

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = GameCategory.List();
        return View(dto);
    }

    // Endpoints de streaming de fotos em tempo de execução
    [HttpGet]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _photoService.GetGamePhotoAsync(id);
        if (photo == null || photo.Image == null || photo.Image.Length == 0)
            return NotFound();

        return File(photo.Image, photo.ContentType);
    }

    [HttpGet]
    public async Task<IActionResult> GetThumbnail(int id)
    {
        var photo = await _photoService.GetGamePhotoAsync(id);
        if (photo == null || (photo.Thumbnail == null && photo.Image == null))
            return NotFound();

        var bytes = photo.Thumbnail ?? photo.Image;
        return File(bytes, photo.ContentType);
    }
}
```

---

## 🎨 Views Razor e Interface do Usuário

### 1. `_ViewImports.cshtml`
Garante que todos os arquivos `.cshtml` tenham acesso direto aos DTOs sem repetição de declarações:

```razor
@using fase_01
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

Voltar para a [[index|Visão Geral]]
