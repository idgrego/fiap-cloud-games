using fase_01.application.dtos;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Dtos
{
    public class JwtSettingsDtoTests
    {
        [Fact]
        public void JwtSettingsDto_ShouldHaveDefaultValuesOnInitialization()
        {
            // Act
            var settings = new JwtSettingsDto();

            // Assert
            settings.SecretKey.Should().BeEmpty();
            settings.Issuer.Should().BeEmpty();
            settings.Audience.Should().BeEmpty();
            settings.ExpirationInMinutes.Should().Be(60);
            settings.RefreshTokenExpirationInDays.Should().Be(30);
        }

        [Fact]
        public void JwtSettingsDto_ShouldSetAndGetPropertiesCorrectly()
        {
            // Act
            var settings = new JwtSettingsDto
            {
                SecretKey = "SuperSecretKey123!",
                Issuer = "FiapIssuer",
                Audience = "FiapAudience",
                ExpirationInMinutes = 120,
                RefreshTokenExpirationInDays = 15
            };

            // Assert
            settings.SecretKey.Should().Be("SuperSecretKey123!");
            settings.Issuer.Should().Be("FiapIssuer");
            settings.Audience.Should().Be("FiapAudience");
            settings.ExpirationInMinutes.Should().Be(120);
            settings.RefreshTokenExpirationInDays.Should().Be(15);
        }
    }
}