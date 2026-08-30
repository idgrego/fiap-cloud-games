using System.Security.Claims;
using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.Controllers;
using fase_01.domain.entities;
using fase_01.domain.interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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

        private void SetUserContext(int userId, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfUsers()
        {
            // Arrange
            SetUserContext(1, isAdmin: true);
            var list = new List<User>();

            for (int i = 1; i <= 5; i++)
                list.Add(new User
                {
                    Id = i,
                    FullName = $"User #{i.ToString()}",
                    Email = $"user{i.ToString()}@test.com"
                });

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
            SetUserContext(1, isAdmin: true);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            SetUserContext(1, isAdmin: false);
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
        public async Task GetById_ShouldReturnForbid_WhenUserIsNotOwnerAndNotAdmin()
        {
            // Arrange
            SetUserContext(1, isAdmin: false);

            // Act
            var result = await _controller.GetById(2);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenLastAdminTriesToRemoveOwnAdminRole()
        {
            // Arrange
            SetUserContext(1, isAdmin: true);
            var existingUser = new User { Id = 1, FullName = "Admin User", Email = "admin@test.com", Admin = true };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(r => r.ListAllAsync()).ReturnsAsync(new List<User> { existingUser });

            var dto = new UserDto { Id = 1, FullName = "Admin User", Email = "admin@test.com", Admin = false };

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenLastAdminTriesToDeleteSelf()
        {
            // Arrange
            SetUserContext(1, isAdmin: true);
            var existingUser = new User { Id = 1, FullName = "Admin User", Email = "admin@test.com", Admin = true };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(r => r.ListAllAsync()).ReturnsAsync(new List<User> { existingUser });

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenOtherAdminExists()
        {
            // Arrange
            SetUserContext(1, isAdmin: true);
            var user1 = new User { Id = 1, FullName = "Admin 1", Email = "admin1@test.com", Admin = true };
            var user2 = new User { Id = 2, FullName = "Admin 2", Email = "admin2@test.com", Admin = true };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user1);
            _userRepositoryMock.Setup(r => r.ListAllAsync()).ReturnsAsync(new List<User> { user1, user2 });

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}