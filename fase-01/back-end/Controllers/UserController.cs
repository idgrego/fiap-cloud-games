using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.dtos;
using fase_01.application.mappings;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace fase_01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;

    public UserController(IUserRepository userRepository, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _photoService = photoService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _userRepository.ListAllAsync();
        var dtos = entities.Select(u => u.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        // obtém o ID do usuário conectado através das Claims
        var currentUserIdClaims = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserIdClaims, out var currentUserId))
            return Forbid();
        if (!(currentUserId == id || User.IsInRole("Admin")))
            return Forbid();

        var entity = await _userRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        return Ok(entity.ToDto());
    }

    #region update item

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto dto)
    {
        if (dto == null || id != dto.Id) 
            ModelState.AddModelError("Id", "O dto.Id não confere com o código do usuário");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _userRepository.GetByIdAsync(dto.Id);
        if (existingEntity == null) return NotFound();

        // Obtém o ID do usuário conectado via Claim
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(currentUserIdClaim, out var currentUserId);

        // Se o usuário logado estiver tentando remover o seu próprio status de Admin
        if (currentUserId == id && existingEntity.Admin && !dto.Admin)
        {
            var allUsers = await _userRepository.ListAllAsync();
            bool hasOtherAdmin = allUsers.Any(u => u.Admin && u.Id != id);

            if (!hasOtherAdmin)
            {
                ModelState.AddModelError("Admin", "Não é possível remover a permissão de administrador do seu próprio usuário enquanto você for o único administrador do sistema.");
                return BadRequest(ModelState);
            }
        }

        var entity = dto.ToEntity(existingEntity);
        await _userRepository.UpdateAsync(entity);
        await _photoService.SaveUserPhotoAsync(entity.Id, dto.PhotoBase64);

        return NoContent();

    }

    #endregion

    #region delete item

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var existingEntity = await _userRepository.GetByIdAsync(id);
        if (existingEntity == null) return NoContent();

        // Obtém o ID do usuário conectado via Claim
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(currentUserIdClaim, out var currentUserId);

        // Se o usuário logado estiver tentando remover o seu próprio status de Admin
        if (currentUserId == id && existingEntity.Admin)
        {
            var allUsers = await _userRepository.ListAllAsync();
            bool hasOtherAdmin = allUsers.Any(u => u.Admin && u.Id != id);

            if (!hasOtherAdmin)
            {
                ModelState.AddModelError("Admin", "Não é possível excluir o seu usuário enquanto você for o único administrador do sistema.");
                return BadRequest(ModelState);
            }
        }

        await _userRepository.DeleteAsync(id);

        // se o próprio usuário se excluiu promove a desconexão do sistema
        if (currentUserId == id) Response.Cookies.Delete("jwt_token");

        return NoContent();
    }

    #endregion


    #region photos

    [HttpGet("photo/{id:int}")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _photoService.GetUserPhotoAsync(id);
        if (photo == null || photo.Image == null || photo.Image.Length == 0)
            return NotFound();

        return File(photo.Image, photo.ContentType);
    }

    [HttpGet("thumbnail/{id:int}")]
    public async Task<IActionResult> GetThumbnail(int id)
    {
        var photo = await _photoService.GetUserPhotoAsync(id);
        if (photo == null || (photo.Thumbnail == null && photo.Image == null))
            return NotFound();

        var bytes = photo.Thumbnail ?? photo.Image;
        return File(bytes, "image/jpeg"); // thumbnail está sendo salvo como jpeg
    }

    [HttpDelete("photo/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var existingEntity = await _userRepository.GetByIdAsync(id);
        if (existingEntity == null) return NoContent();

        // Obtém o ID do usuário conectado via Claim
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserAdminClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        int.TryParse(currentUserIdClaim, out var currentUserId);

        // Se for o próprio usuário ou um administrador tudo bem.
        if (!(currentUserId == id || currentUserAdminClaim == "Admin"))
        {
            ModelState.AddModelError("DefaultErrorMessage", "Apenas o próprio usuário ou um administrador por excluir a foto");
            return BadRequest(ModelState);
        }


        await _photoService.DeleteUserPhotoAsync(id);
        return NoContent();
    }

    #endregion

}
