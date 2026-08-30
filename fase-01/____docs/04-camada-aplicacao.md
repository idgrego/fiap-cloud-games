# 04. Camada de Aplicação (Application Layer)

#application #dtos #validation #extension-methods #imagesharp #csharp

Voltar para a [[index|Visão Geral]] | Ver anterior: [[03-camada-infraestrutura|03. Camada de Infraestrutura]] | Próximo passo: [[05-camada-apresentacao|05. Camada de Apresentação]]

---

## 🎯 Objetivo

A camada de **Aplicação (`_03_application`)** orquestra os casos de uso do sistema. Ela é responsável por expor os DTOs (Data Transfer Objects), executar validações de tela/negócio, realizar o mapeamento entre entidades e DTOs, e processar arquivos de mídia/imagens com redimensionamento de miniaturas.

---

## 📂 Estrutura de Pastas de Aplicação

```
_03_application/
├── dtos/
│   ├── GameDto.cs
│   ├── JwtSettingsDto.cs
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   └── UserDto.cs
├── interfaces/
│   ├── IJwtTokenService.cs
│   ├── IPasswordService.cs
│   └── IPhotoService.cs
├── mappings/
│   ├── GameMappingExtensions.cs
│   └── UserMappingExtensions.cs
└── services/
    ├── JwtTokenService.cs
    ├── PasswordService.cs
    └── PhotoService.cs
```

---

## 📦 DTOs e Validação Cruzada (`IValidatableObject`)

### 1. `GameDto.cs`
Transporta os dados do jogo via requisições `application/json` (`[FromBody]`), incluindo a propriedade `PhotoBase64` em formato de string codificada em Base64.

```csharp
namespace fase_01.application.dtos
{
    using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Photo")]
        public string? PhotoBase64 { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
```

### 2. `UserDto.cs` com Validação de Múltiplas Propriedades
O `UserDto` transporta os dados de usuário via JSON (`[FromBody]`) com suporte a foto em Base64 (`PhotoBase64`) e implementa a interface `IValidatableObject` do ASP.NET Core para garantir que a data de validação (`ValidatedAt`) não seja anterior à data de criação (`CreatedAt`):

```csharp
namespace fase_01.application.dtos
{
    using System.ComponentModel.DataAnnotations;

    public class UserDto : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Fullname")]
        [Required(ErrorMessage = "The field {0} is required")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Nickname")]
        public string? NickName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "The field {0} is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        public bool Admin { get; set; }

        [Display(Name = "Photo")]
        public string? PhotoBase64 { get; set; }

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

### 3. DTOs de Autenticação (`RegisterDto`, `LoginDto`, `JwtSettingsDto`)

```csharp
public class RegisterDto
{
    [Required(ErrorMessage = "The field Fullname is required")]
    public string FullName { get; set; } = string.Empty;

    public string? NickName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required, Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? PhotoBase64 { get; set; }
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class JwtSettingsDto
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 30;
}
```

---

## 🔐 Serviços de Autenticação e Criptografia de Senha

### 1. `PasswordService.cs` (Hash Criptográfico de Senha)
Utiliza `PasswordHasher<User>` do .NET Identity (algoritmo PBKDF2 com HMAC-SHA512 e *salt* único):

```csharp
namespace fase_01.application.services
{
    using fase_01.application.interfaces;
    using fase_01.domain.entities;
    using Microsoft.AspNetCore.Identity;

    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(string password) =>
            _hasher.HashPassword(new User(), password);

        public bool VerifyPassword(string providedPassword, string hashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(new User(), hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
```

### 2. `JwtTokenService.cs` (Geração de Tokens JWT)
Gera o token JWT contendo claims de identidade (`NameIdentifier`, `Name`, `Email`, `nickname`, `Role`):

```csharp
namespace fase_01.application.services
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using fase_01.application.dtos;
    using fase_01.application.interfaces;
    using fase_01.domain.entities;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettingsDto _jwtSettings;

        public JwtTokenService(IOptions<JwtSettingsDto> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(User user, bool rememberMe = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new("nickname", user.NickName ?? string.Empty),
                new(ClaimTypes.Role, user.Admin ? "Admin" : "User")
            };

            var expires = rememberMe
                ? DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
                : DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
```

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

## 🖼️ Processamento de Imagens, Base64 e Miniaturas (`PhotoService.cs`)

O serviço `PhotoService` é responsável por receber imagens codificadas em **Base64** (Data URI scheme `data:image/...;base64,...` ou Base64 puro) via requisições `application/json` (`[FromBody]`), converter os dados para `byte[]`, extrair o `ContentType`, gerar miniaturas de 150x150 pixels mantendo a proporção com **`System.Drawing.Common`**, e fornecer suporte a **exclusão de fotos** de jogos e usuários.

```csharp
namespace fase_01.application.services
{
    using fase_01.application.interfaces;
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;

    public class PhotoService : IPhotoService
    {
        private readonly IGamePhotoRepository _gamePhotoRepository;
        private readonly IUserPhotoRepository _userPhotoRepository;

        public PhotoService(
            IGamePhotoRepository gamePhotoRepository,
            IUserPhotoRepository userPhotoRepository)
        {
            _gamePhotoRepository = gamePhotoRepository;
            _userPhotoRepository = userPhotoRepository;
        }

        #region user's photo

        public async Task SaveUserPhotoAsync(int userId, string? photoBase64)
        {
            if (string.IsNullOrWhiteSpace(photoBase64))
                return;

            var parsedImage = this.ParseBase64Image(photoBase64);
            var thumbnailBytes = await GenerateThumbnailFromBytesAsync(parsedImage.bytes, parsedImage.contentType);

            var userPhoto = new UserPhoto
            {
                Id = userId,
                ContentType = parsedImage.contentType,
                Image = parsedImage.bytes,
                Thumbnail = thumbnailBytes
            };

            await _userPhotoRepository.UpSertAsync(userPhoto);
        }

        public async Task DeleteUserPhotoAsync(int userId)
        {
            await _userPhotoRepository.DeleteAsync(userId);
        }

        #endregion

        #region game's photo

        public async Task SaveGamePhotoAsync(int gameId, string? photoBase64)
        {
            if (string.IsNullOrWhiteSpace(photoBase64))
                return;

            var parsedImage = this.ParseBase64Image(photoBase64);
            var thumbnailBytes = await GenerateThumbnailFromBytesAsync(parsedImage.bytes, parsedImage.contentType);

            var gamePhoto = new GamePhoto
            {
                Id = gameId,
                ContentType = parsedImage.contentType,
                Image = parsedImage.bytes,
                Thumbnail = thumbnailBytes
            };

            await _gamePhotoRepository.UpSertAsync(gamePhoto);
        }

        public async Task DeleteGamePhotoAsync(int gameId)
        {
            await _gamePhotoRepository.DeleteAsync(gameId);
        }

        #endregion

        private (byte[] bytes, string contentType) ParseBase64Image(string base64String)
        {
            if (base64String.Contains(";base64,"))
            {
                var parts = base64String.Split(";base64,");
                var contentType = parts[0].Replace("data:", "");
                var bytes = Convert.FromBase64String(parts[1]);
                return (bytes, contentType);
            }

            return (Convert.FromBase64String(base64String), "image/jpeg");
        }
    }
}
```

            using var thumbnailBitmap = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(thumbnailBitmap);

            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;

            graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);

            using var outputStream = new MemoryStream();
            var imageFormat = file.ContentType.ToLower() switch
            {
                "image/png" => ImageFormat.Png,
                "image/gif" => ImageFormat.Gif,
                _ => ImageFormat.Jpeg
            };

            thumbnailBitmap.Save(outputStream, imageFormat);
            return await Task.FromResult(outputStream.ToArray());
        }
#pragma warning restore CA1416

        private async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
```

Voltar para a [[index|Visão Geral]] | Ver anterior: [[03-camada-infraestrutura|03. Camada de Infraestrutura]] | Próximo passo: [[05-camada-apresentacao|05. Camada de Apresentação]]
