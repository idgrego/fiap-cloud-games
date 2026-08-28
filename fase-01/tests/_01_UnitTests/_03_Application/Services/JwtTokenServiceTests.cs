using fase_01.application.dtos;
using fase_01.application.services;
using fase_01.domain.entities;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace fase_01.tests.UnitTests.Application.Services
{
    public class JwtTokenServiceTests
    {
        private readonly JwtTokenService _jwtTokenService;

        public JwtTokenServiceTests()
        {
            var settings = Options.Create(new JwtSettingsDto
            {
                SecretKey = "ChaveSuperSecretaParaTestesComMaisDe32Caracteres!",
                Issuer = "FiapCloudGames",
                Audience = "FiapUsers",
                ExpirationInMinutes = 60,
                RefreshTokenExpirationInDays = 30
            });

            _jwtTokenService = new JwtTokenService(settings);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtTokenString()
        {
            // Arrange
            var user = new User
            {
                Id = 10,
                FullName = "Test User",
                Email = "test@example.com",
                Admin = true
            };

            // Act
            string token = _jwtTokenService.GenerateToken(user, rememberMe: false);

            // Assert
            token.Should().NotBeNullOrEmpty();
            token.Split('.').Should().HaveCount(3);
        }
    }
}