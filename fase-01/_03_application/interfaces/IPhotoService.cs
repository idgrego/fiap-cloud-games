using fase_01.domain.entities;

namespace fase_01.application.interfaces
{
    public interface IPhotoService
    {
        Task<byte[]?> GetGamePhotoBytesAsync(int gameId);
        Task<GamePhoto?> GetGamePhotoAsync(int gameId);
        Task SaveGamePhotoAsync(int gameId, IFormFile file);
        Task SaveGamePhotoFromBytesAsync(int gameId, byte[] imageBytes, string contentType);

        Task<byte[]?> GetUserPhotoBytesAsync(int userId);
        Task<UserPhoto?> GetUserPhotoAsync(int userId);
        Task SaveUserPhotoAsync(int userId, IFormFile file);
    }
}