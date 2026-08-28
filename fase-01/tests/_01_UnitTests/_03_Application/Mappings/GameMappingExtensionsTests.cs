using fase_01.application.dtos;
using fase_01.application.mappings;
using fase_01.domain.entities;
using fase_01.domain.enums;
using FluentAssertions;

namespace fase_01.tests.UnitTests.Application.Mappings
{
    public class GameMappingExtensionsTests
    {
        [Fact]
        public void GameToDto_ShouldMapAllPropertiescorrectly()
        {
            // Arrange
            var entity = new Game
            {
                Id = 5,
                Name = "Halo Infinite",
                Manufacturer = "Xbox",
                CategoryId = GameCategory.Action.Code,
                Online = true,
                Multiplayer = true
            };

            // Act
            var dto = entity.ToDto();

            // Assert
            dto.Id.Should().Be(entity.Id);
            dto.Name.Should().Be(entity.Name);
            dto.Manufacturer.Should().Be(entity.Manufacturer);
            dto.CategoryId.Should().Be(entity.CategoryId);
            dto.Online.Should().Be(entity.Online);
            dto.Multiplayer.Should().Be(entity.Multiplayer);
        }

        [Fact]
        public void DtoToGame_ShouldMapAllPropertiescorrectly()
        {
            // Arrange
            var dto = new GameDto
            {
                Id = 1,
                Name = "Halo Infinite",
                Manufacturer = "Xbox",
                CategoryId = GameCategory.Action.Code,
                Online = true,
                Multiplayer = true
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            entity.Name.Should().Be(dto.Name);
            entity.Manufacturer.Should().Be(dto.Manufacturer);
            entity.CategoryId.Should().Be(dto.CategoryId);
            entity.Online.Should().Be(dto.Online);
            entity.Multiplayer.Should().Be(dto.Multiplayer);
        }

    }
}