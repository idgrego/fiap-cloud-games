# 04. Camada de Aplicação (Application Layer)

#application #dtos #validation #extension-methods #imagesharp #csharp

Voltar para a [[index|Visão Geral]] | Ver anterior: [[03-camada-infraestrutura|03. Camada de Infraestrutura]]

---

## 🎯 Objetivo

A camada de **Aplicação (`_03_application`)** orquestra os casos de uso do sistema. Ela é responsável por expor os DTOs (Data Transfer Objects), executar validações de tela/negócio, realizar o mapeamento entre entidades e DTOs, e processar arquivos de mídia/imagens com redimensionamento de miniaturas.

---

## 📂 Estrutura de Pastas de Aplicação

```
_03_application/
├── dtos/
│   ├── GameDto.cs
│   └── UserDto.cs
├── interfaces/
│   └── IPhotoService.cs
├── mappings/
│   ├── GameMappingExtensions.cs
│   └── UserMappingExtensions.cs
└── services/
    └── PhotoService.cs
```

---

## 📦 DTOs e Validação Cruzada (`IValidatableObject`)

### 1. `GameDto.cs`
Transporta os dados de formulário do jogo, incluindo a propriedade `IFormFile? Photo` para envio de arquivos de imagem via requisições `multipart/form-data`.

```csharp
namespace fase_01.application.dtos
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    public class GameDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool Online { get; set; }
        public bool Multiplayer { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Category")]
        public byte CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? UrlGame { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? UrlVideo { get; set; }

        public IFormFile? Photo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

### 2. `UserDto.cs` com Validação de Múltiplas Propriedades
O `UserDto` implementa a interface `IValidatableObject` do ASP.NET Core para garantir a regra de negócio onde a data de validação (`ValidatedAt`) não pode ser anterior à data de criação do usuário (`CreatedAt`):

```csharp
namespace fase_01.application.dtos
{
    using System.ComponentModel.DataAnnotations;

    public class UserDto : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Fullname")]
        [Required(ErrorMessage = "The field {0} is required")]
        public string Fullname { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "The field {0} is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        public bool Admin { get; set; }
        public IFormFile? Photo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ValidatedAt { get; set; }

        // Validação executada automaticamente durante o ModelState.IsValid
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ValidatedAt.HasValue && ValidatedAt.Value < CreatedAt)
            {
                yield return new ValidationResult(
                    "The field Validated At must be greater than or equal to Created At.",
                    [nameof(ValidatedAt)]
                );
            }
        }
    }
}
```

---

## 🔄 Mapeamento com Métodos de Extensão C#

Em vez de poluir os Controllers com lógicas repetitivas de conversão ou utilizar bibliotecas pesadas de Reflection como AutoMapper, adotou-se o uso de **Métodos de Extensão Estáticos** (`_03_application/mappings/`):

### Exemplo: `GameMappingExtensions.cs`
```csharp
namespace fase_01.application.mappings
{
    using fase_01.application.dtos;
    using fase_01.domain.entities;

    public static class GameMappingExtensions
    {
        // Converte Entidade -> DTO (uso: game.ToDto())
        public static GameDto ToDto(this Game game)
        {
            return new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                Manufacturer = game.Manufacturer,
                Description = game.Description,
                Online = game.Online,
                Multiplayer = game.Multiplayer,
                CategoryId = game.CategoryId,
                CategoryName = game.Category.Name,
                UrlGame = game.UrlGame,
                UrlVideo = game.UrlVideo,
                CreatedAt = game.CreatedAt
            };
        }

        // Converte DTO -> Entidade (uso: dto.ToEntity())
        public static Game ToEntity(this GameDto dto, Game? existing = null)
        {
            var game = existing ?? new Game();
            game.Name = dto.Name;
            game.Manufacturer = dto.Manufacturer;
            game.Description = dto.Description;
            game.Online = dto.Online;
            game.Multiplayer = dto.Multiplayer;
            game.CategoryId = dto.CategoryId;
            game.UrlGame = dto.UrlGame;
            game.UrlVideo = dto.UrlVideo;
            return game;
        }
    }
}
```

---

## 🖼️ Processamento de Imagens e Miniaturas (`PhotoService.cs`)

O serviço `PhotoService` utiliza a biblioteca **`SixLabors.ImageSharp`** para processar uploads de arquivos `IFormFile`, gerando e redimensionando miniaturas de 150x150 pixels mantendo a proporção de aspecto.

```csharp
namespace fase_01.application.services
{
    using fase_01.application.interfaces;
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class PhotoService : IPhotoService
    {
        private readonly IGamePhotoRepository _gamePhotoRepository;
        private readonly IUserPhotoRepository _userPhotoRepository;

        public PhotoService(IGamePhotoRepository gamePhotoRepository, IUserPhotoRepository userPhotoRepository)
        {
            _gamePhotoRepository = gamePhotoRepository;
            _userPhotoRepository = userPhotoRepository;
        }

        public async Task SaveGamePhotoAsync(int gameId, IFormFile file)
        {
            var imageBytes = await ConvertToBytesAsync(file);
            var thumbnailBytes = await GenerateThumbnailAsync(file);

            var gamePhoto = new GamePhoto
            {
                Id = gameId,
                ContentType = file.ContentType,
                Image = imageBytes,
                Thumbnail = thumbnailBytes
            };

            await _gamePhotoRepository.UpSertAsync(gamePhoto);
        }

        // Processa e redimensiona a miniatura com o ImageSharp
        private async Task<byte[]> GenerateThumbnailAsync(IFormFile file, int width = 150, int height = 150)
        {
            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat!);
            return outputStream.ToArray();
        }

        private async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
```

---

Próximo passo: [[05-camada-apresentacao|05. Camada de Apresentação]]
