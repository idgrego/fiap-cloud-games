using fase_01.domain.entities;

namespace fase_01.domain.interfaces
{
    public interface IUserRepository : IRepositoryBase<User, int>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> RegisterAsync(User entity, string hashedPassword);
        Task<bool> hasAnyUser();
    }
}