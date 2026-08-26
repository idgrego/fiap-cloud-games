using fase_01.domain.entities;

namespace fase_01.application.interfaces
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <param name="rememberMe">Indicates whether the token should be remembered.</param>
        /// <returns>The generated JWT token.</returns>
        string GenerateToken(User user, bool rememberMe = false);
        /// <summary>
        /// Generates a refresh token.
        /// </summary>
        /// <returns>The generated refresh token.</returns>
        string GenerateRefreshToken();
    }
}