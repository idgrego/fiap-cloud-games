using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.Controllers;
using fase_01.domain.entities;
using fase_01.domain.enums;
using fase_01.domain.interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace fase_01.tests.UnitTests.presentation
{
    public class GameControllerTest
    {
        private readonly Mock<IGameRepository> _gameRepositoryMock = new();
        private readonly Mock<IPhotoService> _photoService = new();
        private readonly GameController _controller;

        public GameControllerTest()
        {
            this._controller = new GameController(
                this._gameRepositoryMock.Object,
                this._photoService.Object
            );
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfGames()
        {
            // Arrange
            var list = new List<Game>();
            for (int i = 1; i <= 10; i++)
            {
                list.Add(new Game
                {
                    Id = i,
                    Name = $"Game #{i.ToString()}",
                    Manufacturer = $"Manufacturer #{(i % 2 == 0 ? "A" : "B")}",
                    Online = i % 2 == 0,
                    Multiplayer = i % 3 == 0,
                    CategoryId = GameCategory.FromCode((byte)i).Code,
                    CreatedAt = DateTime.UtcNow.AddMonths(-i)
                });
            }

            // quando o método ListAll for chamado retornar 'list'
            _gameRepositoryMock.Setup(r => r.ListAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var listResult = okResult.Value.Should().BeAssignableTo<IEnumerable<GameDto>>().Subject;
            listResult.Should().HaveCount(list.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(0)).ReturnsAsync((Game?)null);

            // Act
            var result = await _controller.GetById(0);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenGameExists()
        {
            // Arrange
            var item = new Game
            {
                Id = 1,
                Name = $"Game #1",
                Manufacturer = $"Manufacturer #A",
                Online = true,
                Multiplayer = true,
                CategoryId = GameCategory.Action.Code,
                CreatedAt = DateTime.UtcNow.AddMonths(-7)
            };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

            // Act
            var result = await _controller.GetById(item.Id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<GameDto>().Subject;
            dto.Id.Should().Be(item.Id);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");
            var dto = new GameDto();

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenGameIsValid()
        {
            // Arrange
            var dto = new GameDto
            {
                Name = "Game Teste",
                Manufacturer = "A",
                CategoryId = GameCategory.Action.Code,
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be("GetById");
            // confirma que o método AddAsync foi chamado exatamente 1x
            _gameRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
        }

        [Fact]
        public async Task DeleteGame_ShouldReturnNoContent_WhenGameIsDeleted()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            // Confirma que o método DeleteAsync foi invocado exatamente 1x
            _gameRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}