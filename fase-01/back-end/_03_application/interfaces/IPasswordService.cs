using fase_01.domain.entities;

namespace fase_01.application.interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password, User user);
        bool VerifyPassword(string password, string hashedPassword, User user);
    }
}