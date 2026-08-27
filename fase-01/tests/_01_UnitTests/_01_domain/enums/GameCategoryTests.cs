using fase_01.domain.enums;
using FluentAssertions;

namespace fase_01.tests.UnitTests.domain.enums
{
    public class GameCategoryTests
    {
        [Fact]
        public void List_ShouldReturnAllElevenCategories()
        {
            // Arrange

            // Act
            var list = GameCategory.List();

            // Assert
            list.Should().HaveCount(11);
            list.Should().Contain(GameCategory.Action);
        }

        [Theory]
        [InlineData(1, "Action")]
        [InlineData(2, "Adventure")]
        [InlineData(10, "Horror")]
        public void FromCode_ShouldReturnCorrectCategory_WhenCodeIsValid(byte code, string expectedName)
        {
            // Act
            var item = GameCategory.FromCode(code);

            // Assert
            item.Code.Should().Be(code);
            item.Name.Should().Be(expectedName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(99)]
        public void FromCode_ShouldReturnUnknown_WhenCodeIsInvalid(byte code)
        {
            // Act 
            var item = GameCategory.FromCode(code);

            // Assert
            item.Should().Be(GameCategory.Unknown);
        }

        [Theory]
        [InlineData("Action", 1)]
        [InlineData(" ADVENTURE ", 2)]
        [InlineData("horror", 10)]
        public void FromName_ShouldReturnCorrectCategory_WhenNameIsValid(string name, byte expectedCode)
        {
            // Act 
            var item = GameCategory.FromName(name);

            // Assert
            item.Code.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("    ")]
        [InlineData("NonExisting")]
        public void FromName_ShouldReturnUnknown_WhenNameIsInvalid(string? invalidName)
        {
            // Act
            var item = GameCategory.FromName(invalidName!);

            // Assert
            item.Should().Be(GameCategory.Unknown);
        }
    }
}