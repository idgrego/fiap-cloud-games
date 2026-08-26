using fase_01.application.dtos;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Dtos
{
    public class GameDtoTests
    {

        [Fact]
        public void GameDto_ShouldBeValid_WhenAllFieldsAreValid()
        {
            // Arrange
            var dto = new GameDto
            {
                Name = "Halo Infinite",
                Manufacturer = "Xbox Game Studios",
                CategoryId = 1,
                UrlGame = "https://www.xbox.com/halo",
                UrlVideo = "https://www.youtube.com/watch?v=123"
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", "Xbox Game Studios", 1, "Name")]
        [InlineData("Halo Infinite", "", 1, "Manufacturer")]
        public void GameDto_ShouldFailValidation_WhenRequiredFieldsAreMissing(
            string name, string manufacturer, byte categoryId, string expectedErrorMember)
        {
            // Arrange
            var dto = new GameDto
            {
                Name = name,
                Manufacturer = manufacturer,
                CategoryId = categoryId
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains(expectedErrorMember));
        }

        [Theory]
        [InlineData("invalid-url", "https://youtube.com/watch?v=123", "UrlGame")]
        [InlineData("https://xbox.com/game", "not-a-valid-url", "UrlVideo")]
        public void GameDto_ShouldFailValidation_WhenUrlsAreInvalid(
            string urlGame, string urlVideo, string expectedErrorMember)
        {
            // Arrange
            var dto = new GameDto
            {
                Name = "Halo Infinite",
                Manufacturer = "Xbox Game Studios",
                CategoryId = 1,
                UrlGame = urlGame,
                UrlVideo = urlVideo
            };

            // Act
            var results = DtoTests.ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains(expectedErrorMember));
        }
    }
}