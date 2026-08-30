using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.domain.entities;
using fase_01.application.interfaces;
using fase_01.application.dtos;
using fase_01.application.mappings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace fase_01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{

    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettingsDto _jwtSettings;
    private readonly IPhotoService _photoService;

    public AccountController(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettingsDto> jwtSettings,
        IPhotoService photoService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _photoService = photoService;
    }

    #region login/logout

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. busca o usuário pelo e-mail
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // 2. valida a senha
        if (user == null || user.Account == null || !_passwordService.VerifyPassword(dto.Password!, user.Account!.PasswordHash!, user))
            return Unauthorized(new { message = "Invalid email or password." });

        // 3. Gerar o Token JWT
        var token = _jwtTokenService.GenerateToken(user, dto.RememberMe);

        // 4. Configurar as opções do Cookie HTTP-Only
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Impede acesso via JavaScript (mitiga XSS)
            Secure = true,   // Exige HTTPS
            SameSite = SameSiteMode.Strict, // Mitiga CSRF
            Expires = dto.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
                : DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
        };

        // 5. Salvar o Token no Cookie "jwt_token"
        Response.Cookies.Append("jwt_token", token, cookieOptions);

        return Ok(new { token, user = user.ToDto() });

    }

    // POST: /Account/Logout
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        // Remove o Cookie HTTP contendo o JWT
        Response.Cookies.Delete("jwt_token");

        return Ok(new { message = "Logout realizado com sucesso" });
    }


    #endregion

    #region registration

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Validar se o e-mail já existe no banco
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Este e-mail já está em uso por outro usuário." });

        // 2. Criar o usuário
        var entity = dto.ToEntity();
        // garante que o 1o usuário seja o administrador do sistema
        if (!await this._userRepository.hasAnyUser())
        {
            entity.Admin = true;
            entity.ValidatedAt = DateTime.UtcNow;
        }

        // 3. Criptografar a senha (gera o Hash seguro)
        var passwordHash = _passwordService.HashPassword(dto.Password!, entity);

        // 4. Salvar o usuário no banco de dados
        await _userRepository.RegisterAsync(entity, passwordHash);

        // 5. Salvar a foto do usuário, se houver
        if (!string.IsNullOrWhiteSpace(dto.PhotoBase64))
            await _photoService.SaveUserPhotoAsync(entity.Id, dto.PhotoBase64);

        // 6. Devolve o resultado
        return CreatedAtAction(nameof(Register), new { id = entity.Id }, entity.ToDto());

    }

    #endregion

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException("Teste de exceção não tratada capturada pelo Middleware!");
    }

}
