using fase_01.application.interfaces;
using fase_01.domain.entities;
using Microsoft.AspNetCore.Identity;

namespace fase_01.application.services
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(string password, User user)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(string password, string hashedPassword, User user)
        {
            PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, hashedPassword, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}