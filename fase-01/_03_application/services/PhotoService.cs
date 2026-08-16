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
    }
}