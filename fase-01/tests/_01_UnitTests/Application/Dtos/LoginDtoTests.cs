using fase_01.application.dtos;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Dtos
{
    public class LoginDtoTests
    {

        [Fact]
        public void LoginDto_ShouldBeValid_WhenEmailAndPasswordAreProvided()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "john.doe@example.com",
                Password = "Password123!",
                RememberMe = true
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", "senha", "Email")]
        [InlineData("not-an-email", "senha", "Email")]
        [InlineData("john.doe@example.com", "", "Password")]
        public void LoginDto_ShouldFailValidation_WhenFieldsAreMissingOrInvalid(
            string email,
            string password,
            string expectedErrorMember
        )
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains(expectedErrorMember));
        }
    }
}