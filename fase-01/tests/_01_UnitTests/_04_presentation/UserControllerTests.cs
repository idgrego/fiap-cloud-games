using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.Controllers;
using fase_01.domain.entities;
using fase_01.domain.interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace fase_01.tests.UnitTests.presentation
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPhotoService> _photoServiceMock = new();
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _controller = new UserController(
                _userRepositoryMock.Object,
                _photoServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfUsers()
        {
            // Arrange

            var list = new List<User>();

            for (int i = 1; i <= 5; i++)
                list.Add(new User
                {
                    Id = i,
                    FullName = $"User #{i.ToString()}",
                    Email = $"user{i.ToString()}@test.com"
                });

            // quando o método ListAll for invocado retornar o valor list
            _userRepositoryMock.Setup(r => r.ListAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            returnedDtos.Should().HaveCount(list.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByIdAsync(0)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetById(0);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var item = new User { Id = 1, FullName = "User #1", Email = "user1@test.com" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(item);

            // Act
            var result = await _controller.GetById(item.Id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<UserDto>().Subject;
            dto.Id.Should().Be(item.Id);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>();
        }
    }
}