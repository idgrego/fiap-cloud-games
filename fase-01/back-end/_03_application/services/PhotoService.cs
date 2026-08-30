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

        #region user's photo
        public async Task SaveUserPhotoAsync(int userId, string? photoBase64)
        {
            if (string.IsNullOrWhiteSpace(photoBase64))
                return;

            var parsedImage = this.ParseBase64Image(photoBase64);

            var userPhoto = new UserPhoto
            {
                Id = userId,
                ContentType = parsedImage.contentType,
                Image = parsedImage.bytes,
                Thumbnail = await GenerateThumbnailAsync(parsedImage.bytes)
            };

            await _userPhotoRepository.UpSertAsync(userPhoto);
        }

        public async Task<UserPhoto?> GetUserPhotoAsync(int userId)
        {
            return await _userPhotoRepository.GetByIdAsync(userId);
        }

        public async Task DeleteUserPhotoAsync(int userId)
        {
            await _userPhotoRepository.DeleteAsync(userId);
        }

        #endregion

        #region game's photo
        public async Task SaveGamePhotoAsync(GamePhoto gamePhoto)
        {
            if (gamePhoto == null) return;
            await _gamePhotoRepository.UpSertAsync(gamePhoto);
        }
        public async Task SaveGamePhotoAsync(int gameId, string? photoBase64)
        {
            if (string.IsNullOrWhiteSpace(photoBase64))
                return;

            var parsedImage = this.ParseBase64Image(photoBase64);

            var gamePhoto = new GamePhoto
            {
                Id = gameId,
                ContentType = parsedImage.contentType,
                Image = parsedImage.bytes,
                Thumbnail = await GenerateThumbnailAsync(parsedImage.bytes)
            };

            await this.SaveGamePhotoAsync(gamePhoto);
        }

        public async Task<GamePhoto?> GetGamePhotoAsync(int gameId)
        {
            return await _gamePhotoRepository.GetByIdAsync(gameId);
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
                // Exemplo de string: "data:image/png;base64,iVBORw0KGgoAAAAN..."
                var parts = base64String.Split(";base64,");

                // Extrai o Content-Type: "image/png"
                var contentType = parts[0].Replace("data:", "");

                // Converte a parte dos dados em byte[]
                var imgBytes = Convert.FromBase64String(parts[1]);

                return (imgBytes, contentType);
            }

            return (Convert.FromBase64String(base64String), "image/jpeg");
        }

        public static async Task<GamePhoto?> ScrapGameImageAsync(int gameId, string urlGame)
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

                    var gamePhoto = new GamePhoto
                    {
                        Id = gameId,
                        ContentType = GetContentTypeFromBytes(imageBytes, "image/jpeg"),
                        Image = imageBytes,
                        Thumbnail = await GenerateThumbnailAsync(imageBytes)
                    };

                    return gamePhoto;

                }
            }
            catch
            {
                // Se houver timeout, 404 ou erro de rede, apenas ignora para não travar a página
            }

            return null;
        }

        private static string GetContentTypeFromBytes(byte[] bytes, string defaultContentType = "application/octet-stream")
        {
            if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
                return "image/jpeg";

            if (bytes.Length >= 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                return "image/gif";

            return defaultContentType; // Tipo padrão caso não identifique
        }

#pragma warning disable CA1416

        private static async Task<byte[]> GenerateThumbnailAsync(byte[] imageBytes, int width = 150, int height = 150)
        {
            using var inputStream = new MemoryStream(imageBytes);
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
            thumbnailBitmap.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            return await Task.FromResult(outputStream.ToArray());
        }


#pragma warning restore CA1416

    }
}