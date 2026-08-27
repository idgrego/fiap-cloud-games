using fase_01.application.dtos;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Dtos
{
    public class RegisterDtoTests
    {

        [Fact]
        public void RegisterDto_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "John Doe",
                NickName = "johnd",
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", "johnd", "john.doe@example.com", "Password123!", "Password123!", "FullName")]
        [InlineData("John Doe", "johnd", "", "Password123!", "Password123!", "Email")]
        [InlineData("John Doe", "johnd", "invalid-email-format", "Password123!", "Password123!", "Email")]
        public void RegisterDto_ShouldFailValidation_WhenRequiredOrEmailFieldsAreInvalid(
            string fullName,
            string? nickName,
            string email,
            string password,
            string confirmPassword,
            string expectedErrorMember
        )
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = fullName,
                NickName = nickName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains(expectedErrorMember));
        }

        [Theory]
        [InlineData("Short1!")] // menos de 8 caracteres
        [InlineData("lowercase123!")] // sem letras maiúsculas
        [InlineData("UPPERCASE123!")] // sem letras minusculas
        [InlineData("NoNumber!")] // sem números
        [InlineData("NoSpecialChar123")] // sem caracteres especiais
        public void RegisterDto_ShouldFailValidation_WhenPasswordDoesNotMeetComplexity(string invalidPassword)
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = invalidPassword,
                ConfirmPassword = invalidPassword
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterDto_ShouldFailValidation_WhenConfirmPasswordDoesNotMatch()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("ConfirmPassword"));
        }
    }
}