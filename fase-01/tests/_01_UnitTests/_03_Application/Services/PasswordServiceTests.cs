using fase_01.application.services;
using fase_01.domain.entities;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _passwordService = new();
        private readonly User _user = new() { Id = 1, Email = "test@example.com" };

        [Fact]
        public void HashPassword_ShouldReturnValidHash_WhenPasswordIsProvided()
        {
            // Arrange
            string pwd = "SenhaSecreta!123";

            // Act
            string hash = _passwordService.HashPassword(pwd, this._user);

            // Assert
            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotBe(pwd);
            hash.Should().HaveLength(84);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            // Arrange
            string pwd = "SenhaSecreta!123";
            string hash = _passwordService.HashPassword(pwd, _user);

            // Act
            bool isValid = _passwordService.VerifyPassword(pwd, hash, _user);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            string correctPassword = "SenhaSecreta!123";
            string incorrectPassword = "WringPassword123!";
            string hash = _passwordService.HashPassword(correctPassword, _user);

            // Act
            bool isValid = _passwordService.VerifyPassword(incorrectPassword, hash, _user);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}