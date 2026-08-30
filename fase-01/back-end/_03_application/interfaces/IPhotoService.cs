using fase_01.domain.entities;

namespace fase_01.application.interfaces
{
    public interface IPhotoService
    {
        // game's photo
        Task<GamePhoto?> GetGamePhotoAsync(int gameId);
        Task DeleteGamePhotoAsync(int gameId);
        Task SaveGamePhotoAsync(int gameId, string? photoBase64);
        Task SaveGamePhotoAsync(GamePhoto gamePhoto);

        // user's photo
        Task<UserPhoto?> GetUserPhotoAsync(int userId);
        Task DeleteUserPhotoAsync(int userId);
        Task SaveUserPhotoAsync(int userId, string? photoBase64);
    }
}