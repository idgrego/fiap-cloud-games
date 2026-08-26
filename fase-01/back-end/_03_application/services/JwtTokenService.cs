using System.IdentityModel.Tokens.Jwt;
using System.Text;
using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.domain.entities;
using Microsoft.Extensions.Options;

namespace fase_01.application.services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettingsDto _jwtSettings;

        public JwtTokenService(IOptions<JwtSettingsDto> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public string GenerateToken(User user, bool rememberMe = false)
        {
            byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            // Clains são as declarações que serão incluídas no token. Aqui você pode adicionar informações relevantes sobre o usuário.
            // ** Conceito importante sobre Claims: **
            // As claims são pares chave-valor codificados dentro do token JWT.
            // Quando o usuário faz requisições futuras, o ASP.NET descriptografa
            // o token e preenche o objeto User.Identity na Controller automaticamente,
            // permitindo ler quem está logado e quais suas permissões
            // sem precisar consultar o banco a cada requisição.
            System.Security.Claims.Claim[] claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Adicione outras claims conforme necessário
                /* new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new("nickname", user.NickName ?? string.Empty),
                new(System.Security.Claims.ClaimTypes.Role, user.Admin ? "Admin" : "User") */
            };

            // Aqui você pode definir a expiração do token com base na configuração de ExpirationInMinutes
            DateTime expiration = rememberMe
                ? DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
                : DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature
                ),
            };

            JwtSecurityTokenHandler tokenHandler = new();
            Microsoft.IdentityModel.Tokens.SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}