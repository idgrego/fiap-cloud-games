using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.dtos;
using fase_01.application.mappings;
using Microsoft.AspNetCore.Authorization;

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
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
    public async Task<IActionResult> Update(int id, UserDto dto)
    {

        if (ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _userRepository.GetByIdAsync(dto.Id);
        if (existingEntity == null) return NotFound();

        var entity = dto.ToEntity(existingEntity);
        await _userRepository.UpdateAsync(entity);

        if (dto.Photo != null && dto.Photo.Length > 0)
            await _photoService.SaveUserPhotoAsync(entity.Id, dto.Photo);

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
        await _userRepository.DeleteAsync(id);
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
        return File(bytes, photo.ContentType);
    }

    #endregion

}
