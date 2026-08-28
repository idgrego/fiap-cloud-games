using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.Controllers;
using fase_01.domain.entities;
using fase_01.domain.interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace fase_01.tests.UnitTests.presentation
{
    public class AccountControllerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IPasswordService> _pwdServiceMock = new();
        private readonly Mock<IJwtTokenService> _jwtServiceMock = new();
        private readonly Mock<IPhotoService> _photoServiceMock = new();
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var options = Options.Create(new JwtSettingsDto
            {
                SecretKey = "ChaveSuperSecretaDeTestesComMaisDe32Caracteres!",
                ExpirationInMinutes = 60,
                RefreshTokenExpirationInDays = 30
            });

            _controller = new AccountController(
                _userRepoMock.Object,
                _pwdServiceMock.Object,
                _jwtServiceMock.Object,
                options,
                _photoServiceMock.Object
            );

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        #region testes de login

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "The Email field is required");
            var dto = new LoginDto();

            // Act 
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorizes_WhenUserNotFound()
        {
            // Arrange
            var dto = new LoginDto { Email = "not.found@test.com", Password = "SenhaSecreta123!" };
            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

            // Act 
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorizes_WhenPasswordIsInvalid()
        {
            // Arrange
            var dto = new LoginDto { Email = "user@test.com", Password = "WrongPassword123!" };
            var user = new User { Id = 1, Email = dto.Email, Account = new Account { PasswordHash = "hashed_pass" } };

            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _pwdServiceMock.Setup(p => p.VerifyPassword(dto.Password, user.Account.PasswordHash, user)).Returns(false);

            // Act 
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Login_ShouldReturnOkWithToken_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginDto { Email = "user@test.com", Password = "SenhaSecreta123!" };
            var user = new User { Id = 1, Email = dto.Email, Account = new Account { PasswordHash = "hashed_pass" } };

            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _pwdServiceMock.Setup(p => p.VerifyPassword(dto.Password, user.Account.PasswordHash, user)).Returns(true);
            _jwtServiceMock.Setup(j => j.GenerateToken(user, dto.RememberMe)).Returns("mocked_jwt_token");

            // Act 
            var result = await _controller.Login(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        #endregion

        #region testes de logout

        [Fact]
        public void Logout_ShouldReturnOk_AndRemoveCookie()
        {
            // Act
            var result = _controller.Logout();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        #endregion

        #region testes de registro

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("FullName", "Required");
            var dto = new RegisterDto();

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = new RegisterDto { FullName = "Existing user", Email = "exists@test.com", Password = "Password123!" };
            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new User { Email = dto.Email });

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var dto = new RegisterDto { FullName = "New User", Email = "new@test.com", Password = "Password123!" };
            _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            _userRepoMock.Setup(r => r.hasAnyUser()).ReturnsAsync(true);
            _pwdServiceMock.Setup(r => r.HashPassword(It.IsAny<string>(), It.IsAny<User>()));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be("Register");
        }

        #endregion

    }
}