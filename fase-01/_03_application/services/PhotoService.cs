namespace fase_01.application.services
{
    using System.Text.RegularExpressions;
    using fase_01.application.interfaces;
    using fase_01.domain.entities;
    using fase_01.domain.interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

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

        public async Task SaveUserPhotoAsync(int userId, IFormFile file)
        {
            var imageBytes = await ConvertToBytesAsync(file);
            var thumbnailBytes = await GenerateThumbnailAsync(file);
            var userPhoto = new UserPhoto
            {
                Id = userId,
                ContentType = file.ContentType,
                Image = imageBytes,
                Thumbnail = thumbnailBytes
            };

            await _userPhotoRepository.UpSertAsync(userPhoto);
        }

        public async Task<GamePhoto?> GetGamePhotoAsync(int gameId)
        {
            return await _gamePhotoRepository.GetByIdAsync(gameId);
        }

        public async Task<UserPhoto?> GetUserPhotoAsync(int userId)
        {
            return await _userPhotoRepository.GetByIdAsync(userId);
        }

        public async Task<byte[]?> GetGamePhotoBytesAsync(int gameId)
        {
            var gamePhoto = await _gamePhotoRepository.GetByIdAsync(gameId);
            return gamePhoto?.Image;
        }

        public async Task<byte[]?> GetUserPhotoBytesAsync(int userId)
        {
            var userPhoto = await _userPhotoRepository.GetByIdAsync(userId);
            return userPhoto?.Image;
        }

        // Gera a miniatura mantendo a proporção ou recortando proporcionalmente
        private async Task<byte[]> GenerateThumbnailAsync(IFormFile file, int width = 150, int height = 150)
        {
            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            // Redimensiona mantendo proporções (ResizeMode.Max ou ResizeMode.Crop)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            using var outputStream = new MemoryStream();
            // Salva no mesmo formato/encoder detectado ou em JPEG/PNG
            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat!);
            return outputStream.ToArray();
        }

        private async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public static async Task<byte[]?> ScrapGameImageAsync(string urlGame)
        {

            // implementando pela IA

            try
            {
                using var client = new HttpClient();
                // Define um User-Agent para evitar bloqueio por parte de alguns sites
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                // 1. Baixa o HTML da página oficial do jogo
                var html = await client.GetStringAsync(urlGame);

                // 2. Procura pela meta tag OpenGraph og:image no HTML
                var match = Regex.Match(html, @"<meta[^>]+property=[""']og:image[""'][^>]+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    // Tentativa alternativa com name="og:image" ou Twitter card
                    match = Regex.Match(html, @"<meta[^>]+content=[""']([^""']+)[""'][^>]+property=[""']og:image[""']", RegexOptions.IgnoreCase);
                }

                if (match.Success)
                {
                    string imageUrl = match.Groups[1].Value;

                    // Resolve URLs relativas caso necessário
                    if (!imageUrl.StartsWith("http"))
                    {
                        var uri = new Uri(urlGame);
                        imageUrl = new Uri(uri, imageUrl).ToString();
                    }

                    // 3. Baixa os bytes da imagem encontrada
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);
                    return imageBytes;
                }
            }
            catch
            {
                // Se houver timeout, 404 ou erro de rede, apenas ignora para não travar a página
            }

            return null;
        }

        public async Task SaveGamePhotoFromBytesAsync(int gameId, byte[] imageBytes, string contentType = "image/jpeg")
        {
            // Gera a miniatura com o ImageSharp a partir do array de bytes
            using var image = Image.Load(imageBytes);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(150, 150),
                Mode = ResizeMode.Max
            }));

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat ?? SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance);

            var gamePhoto = new GamePhoto
            {
                Id = gameId,
                ContentType = contentType,
                Image = imageBytes,
                Thumbnail = outputStream.ToArray()
            };

            await _gamePhotoRepository.UpSertAsync(gamePhoto);
        }
    }
}