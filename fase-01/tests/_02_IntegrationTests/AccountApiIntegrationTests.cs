using System.Net;
using System.Net.Http.Json;
using fase_01.application.dtos;
using FluentAssertions;

namespace fase_01.tests.IntegrationTestes
{
    public class AccountApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AccountApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            // Cria um HttpClient que faz requisições reais para a Web API rodando em memória
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturn201Created_WhenUserPayloadIsValid()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FullName = "Integration User",
                NickName = "intuser",
                Email = "integration.user@test.com",
                Password = "ValidPassword123!",
                ConfirmPassword = "ValidPassword123!"
            };

            // Act - Dispara o HTTP Post real para a rota /api/account/register
            var response = await _client.PostAsJsonAsync("/api/account/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
            createdUser.Should().NotBeNull();
            createdUser.Email.Should().Be(registerDto.Email);
            createdUser.FullName.Should().Be(registerDto.FullName);
        }

        [Fact]
        public async Task Register_ShouldReturn400BadRequest_WhenPasswordIsWeak()
        {
            // Arrange 
            var registerDto = new RegisterDto
            {
                FullName = "Weak User",
                Email = "weak@test.com",
                Password = "123", // senha fraca
                ConfirmPassword = "123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ShouldReturn200Ok_WithJwtToken_WhenCredentialsAreValid()
        {
            // Arrange #1 - cadastra o usuário
            var registerDto = new RegisterDto
            {
                FullName = "Login User",
                Email = "login.test@example.com",
                Password = "ComplexPassword123!",
                ConfirmPassword = "ComplexPassword123!"
            };
            await _client.PostAsJsonAsync("/api/account/register", registerDto);

            // Arrange #2 - prepara o payload do login
            var loginDto = new LoginDto
            {
                Email = registerDto.Email,
                Password = registerDto.Password,
                RememberMe = true
            };

            // Act - dispara o HTTP POST para /api/account/login
            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<LoginResponseResult>();
            responseContent.Should().NotBeNull();
            responseContent.Token.Should().NotBeNullOrWhiteSpace();
            responseContent.User.Email.Should().Be(loginDto.Email);

            // verifica se o cookie "jwt_token" HTTP-Only foi anexado na resposta HTTP
            response.Headers.Should().ContainKey("Set-Cookie");
        }

        [Fact]
        public async Task Login_ShouldReturn401Unauthorized_WhenPasswordIsWrong()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "login.test@example.com",
                Password = "InvalidPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Classe auxiliar DTO para deserializar a resposta do endpoint de Login
        private class LoginResponseResult
        {
            public string Token { get; set; } = string.Empty;
            public UserDto User { get; set; } = new();
        }
    }
}