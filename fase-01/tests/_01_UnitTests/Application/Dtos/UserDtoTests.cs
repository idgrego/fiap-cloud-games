using fase_01.application.dtos;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Dtos
{
    public class UserDtoTests
    {

        [Fact]
        public void UserDto_ShouldBeValid_WhenValidatedAtIsGreaterThanOrEqualToCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new UserDto
            {
                FullName = "Valid User",
                Email = "user@example.com",
                CreatedAt = now,
                ValidatedAt = now.AddMinutes(5)
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void UserDto_ShouldBeValid_WhenValidatedAtIsNull()
        {
            // Arrange
            var dto = new UserDto
            {
                FullName = "Valid User",
                Email = "user@example.com",
                CreatedAt = DateTime.UtcNow,
                ValidatedAt = null
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void UserDto_ShouldFailValidation_WhenValidatedAtIsEarlierThanCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new UserDto
            {
                FullName = "Valid User",
                Email = "user@example.com",
                CreatedAt = now,
                ValidatedAt = now.AddMinutes(-10) // Data de validação anterior à criação
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("ValidatedAt"));
        }
    }
}