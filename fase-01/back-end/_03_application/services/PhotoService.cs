namespace fase_01.application.services
{
    using System.Text.RegularExpressions;
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
#pragma warning disable CA1416 // Valida a compatibilidade da plataforma

        private static async Task<byte[]> GenerateThumbnailAsync(IFormFile file, int width = 150, int height = 150)
        {
            using var inputStream = file.OpenReadStream();
            using var originalBitmap = new System.Drawing.Bitmap(inputStream);

            // Calcula as proporções para manter o aspect ratio da imagem
            float ratioX = (float)width / originalBitmap.Width;
            float ratioY = (float)height / originalBitmap.Height;
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalBitmap.Width * ratio);
            int newHeight = (int)(originalBitmap.Height * ratio);

            // Cria o novo Bitmap para a miniatura redimensionada
            using var thumbnailBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
            using var graphics = System.Drawing.Graphics.FromImage(thumbnailBitmap);

            // Configura alta qualidade para a interpolação do redimensionamento
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);

            using var outputStream = new MemoryStream();

            // Define o formato de saída baseado na extensão do arquivo (padrão JPEG)
            var imageFormat = file.ContentType.ToLower() switch
            {
                "image/png" => System.Drawing.Imaging.ImageFormat.Png,
                "image/gif" => System.Drawing.Imaging.ImageFormat.Gif,
                _ => System.Drawing.Imaging.ImageFormat.Jpeg
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

#pragma warning disable CA1416
        public async Task SaveGamePhotoFromBytesAsync(int gameId, byte[] imageBytes, string contentType = "image/jpeg")
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var originalBitmap = new System.Drawing.Bitmap(inputStream);

            float ratioX = 150f / originalBitmap.Width;
            float ratioY = 150f / originalBitmap.Height;
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalBitmap.Width * ratio);
            int newHeight = (int)(originalBitmap.Height * ratio);

            using var thumbnailBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
            using var graphics = System.Drawing.Graphics.FromImage(thumbnailBitmap);

            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);

            using var outputStream = new MemoryStream();
            thumbnailBitmap.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            var gamePhoto = new GamePhoto
            {
                Id = gameId,
                ContentType = contentType,
                Image = imageBytes,
                Thumbnail = outputStream.ToArray()
            };

            await _gamePhotoRepository.UpSertAsync(gamePhoto);
        }
#pragma warning restore CA1416

    }
}