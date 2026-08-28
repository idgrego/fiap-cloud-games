using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.mappings;
using fase_01.application.dtos;
using fase_01.application.services;
using Microsoft.AspNetCore.Authorization;

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

        // Verifica se a foto já existe no banco
        var photo = await _photoService.GetGamePhotoAsync(id);
        // Se NÃO existir foto e houver uma URL de jogo configurada, faz o scraping automático
        if (photo == null && !string.IsNullOrWhiteSpace(entity.UrlGame))
        {
            var downloadedBytes = await PhotoService.ScrapGameImageAsync(entity.UrlGame);
            if (downloadedBytes != null && downloadedBytes.Length > 0)
                await _photoService.SaveGamePhotoFromBytesAsync(entity.Id, downloadedBytes, "image/jpeg");
        }

        return Ok(entity.ToDto());
    }

    #region create item

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<GameDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] GameDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = dto.ToEntity();
        await _gameRepository.AddAsync(entity);

        if (dto.Photo != null && dto.Photo.Length > 0)
            await _photoService.SaveGamePhotoAsync(entity.Id, dto.Photo);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());


        //ViewBag.Categories = GameCategory.List().Select(i => new SelectListItem(i.Name, i.Code.ToString(), i.Code == dto.CategoryId)).ToList();
    }

    #endregion

    #region update item

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromForm] GameDto dto)
    {
        if (ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _gameRepository.GetByIdAsync(dto.Id);
        if (existingEntity == null) return NotFound();

        var entity = dto.ToEntity(existingEntity);
        await _gameRepository.UpdateAsync(entity);

        if (dto.Photo != null && dto.Photo.Length > 0)
            await _photoService.SaveGamePhotoAsync(entity.Id, dto.Photo);

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
        return File(bytes, photo.ContentType);
    }

    #endregion

}
