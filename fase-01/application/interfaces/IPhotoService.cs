namespace fase_01.application.interfaces
{
    public interface IPhotoService
    {
        Task SaveGamePhotoAsync(int gameId, IFormFile file);
        Task SaveUserPhotoAsync(int userId, IFormFile file);
        Task<byte[]?> GetGamePhotoBytesAsync(int gameId);
        Task<byte[]?> GetUserPhotoBytesAsync(int userId);
    }
}