using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.domain.entities;
using fase_01.application.interfaces;
using fase_01.application.dtos;
using fase_01.application.mappings;
using Microsoft.Extensions.Options;

namespace fase_01.Controllers;

public class AccountController : Controller
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

    [HttpGet]
    public IActionResult Login()
    {
        // Se o usuário já estiver autenticado, redireciona para a Home
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        // 1. busca o usuário pelo e-mail
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // 2. valida a senha
        if (user == null || user.Account == null || !_passwordService.VerifyPassword(dto.Password!, user.Account!.PasswordHash!, user))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(dto);
        }

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

        TempData["SuccessMessage"] = $"Bem-vindo de volta, {user.FullName}!";

        return RedirectToAction("Index", "Home");

    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        // Remove o Cookie HTTP contendo o JWT
        Response.Cookies.Delete("jwt_token");

        TempData["SuccessMessage"] = "Você saiu do sistema.";

        return RedirectToAction("Login");
    }


    #endregion

    #region registration

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrer(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        // 1. Validar se o e-mail já existe no banco
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Este e-mail já está em uso por outro usuário.");
            return View(dto);
        }

        // 2. Criar o usuário
        var entity = dto.ToEntity();
        // garante que o 1o usuário seja o administrador do sistema
        entity.Admin = !await this._userRepository.hasAnyUser();

        // 3. Criptografar a senha (gera o Hash seguro)
        var passwordHash = _passwordService.HashPassword(dto.Password!, entity);

        // 4. Salvar o usuário no banco de dados
        await _userRepository.RegisterAsync(entity, passwordHash);

        // 5. Salvar a foto do usuário, se houver
        if (dto.Photo != null && dto.Photo.Length > 0)
            await _photoService.SaveUserPhotoAsync(entity.Id, dto.Photo);

        TempData["SuccessMessage"] = "Registration completed successfully. Please log in.";

        // 6. Redirecionar para a página de login
        return RedirectToAction(nameof(Login));

    }

    #endregion

}
