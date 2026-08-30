using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.mappings;
using fase_01.application.dtos;
using fase_01.application.services;
using Microsoft.AspNetCore.Authorization;
using fase_01.domain.entities;

namespace fase_01.Controllers;

[ApiController()]
[Route("api/[controller]")]
public class GameController : ControllerBase
{

    private readonly IGameRepository _gameRepository;
    private readonly IPhotoService _photoService;

    public GameController(IGameRepository gameRepository, IPhotoService photoService)
    {
        _gameRepository = gameRepository;
        _photoService = photoService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<GameDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _gameRepository.ListAllAsync();
        var dtos = entities.Select(g => g.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<GameDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _gameRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        if (entity.Photo == null && !string.IsNullOrWhiteSpace(entity.UrlGame))
        {
            // Se NÃO existir foto e houver uma URL de jogo configurada, faz o scraping automático
            GamePhoto? newGamePhoto = await PhotoService.ScrapGameImageAsync(id, entity.UrlGame);
            if (newGamePhoto != null) await _photoService.SaveGamePhotoAsync(newGamePhoto);
        }

        return Ok(entity.ToDto());
    }

    #region create item

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<GameDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] GameDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = dto.ToEntity();
        await _gameRepository.AddAsync(entity);
        await _photoService.SaveGamePhotoAsync(entity.Id, dto.PhotoBase64);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    #endregion

    #region update item

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] GameDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _gameRepository.GetByIdAsync(dto.Id);
        if (existingEntity == null) return NotFound();

        var entity = dto.ToEntity(existingEntity);
        await _gameRepository.UpdateAsync(entity);
        await _photoService.SaveGamePhotoAsync(entity.Id, dto.PhotoBase64);

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
        await _gameRepository.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region photos

    [HttpGet("photo/{id:int}")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _photoService.GetGamePhotoAsync(id);
        if (photo == null || photo.Image == null || photo.Image.Length == 0)
            return NotFound();

        return File(photo.Image, photo.ContentType);
    }

    [HttpGet("thumbnail/{id:int}")]
    public async Task<IActionResult> GetThumbnail(int id)
    {
        var photo = await _photoService.GetGamePhotoAsync(id);
        if (photo == null || (photo.Thumbnail == null && photo.Image == null))
            return NotFound();

        var bytes = photo.Thumbnail ?? photo.Image;
        return File(bytes, "image/jpeg"); // thumbnail está sendo salvo como jpeg
    }

    [HttpDelete("photo/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        await _photoService.DeleteGamePhotoAsync(id);
        return NoContent();
    }

    #endregion

}
