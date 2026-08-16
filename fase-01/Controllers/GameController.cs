using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.application.interfaces;
using fase_01.application.mappings;
using fase_01.domain.enums;
using fase_01.application.dtos;
using fase_01.application.services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace fase_01.Controllers;

public class GameController : Controller
{

    private readonly IGameRepository _gameRepository;
    private readonly IPhotoService _photoService;

    public GameController(IGameRepository gameRepository, IPhotoService photoService)
    {
        _gameRepository = gameRepository;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var entities = await _gameRepository.ListAllAsync();
        var dtos = entities.Select(g => g.ToDto());
        return View(dtos);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
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

        return View(entity.ToDto());
    }

    #region create item

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = GameCategory.List().Select(i => new SelectListItem(i.Name, i.Code.ToString())).ToList();
        return View(new GameDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GameDto dto)
    {
        if (ModelState.IsValid)
        {
            var entity = dto.ToEntity();
            await _gameRepository.AddAsync(entity);

            if (dto.Photo != null && dto.Photo.Length > 0)
                await _photoService.SaveGamePhotoAsync(entity.Id, dto.Photo);

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = GameCategory.List().Select(i => new SelectListItem(i.Name, i.Code.ToString(), i.Code == dto.CategoryId)).ToList();
        return View(dto);
    }

    #endregion

    #region update item

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var entity = await _gameRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        ViewBag.Categories = GameCategory.List().Select(i => new SelectListItem(i.Name, i.Code.ToString(), i.Code == entity.CategoryId)).ToList();
        return View(entity.ToDto());
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(GameDto dto)
    {
        if (ModelState.IsValid)
        {
            var existingEntity = await _gameRepository.GetByIdAsync(dto.Id);
            if (existingEntity == null) return NotFound();

            var entity = dto.ToEntity(existingEntity);
            await _gameRepository.UpdateAsync(entity);

            if (dto.Photo != null && dto.Photo.Length > 0)
                await _photoService.SaveGamePhotoAsync(entity.Id, dto.Photo);

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = GameCategory.List().Select(i => new SelectListItem(i.Name, i.Code.ToString(), i.Code == dto.CategoryId)).ToList();
        return View(dto);
    }

    #endregion

    #region delete item

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _gameRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity.ToDto());
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _gameRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region photos

    [HttpGet]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _photoService.GetGamePhotoAsync(id);
        if (photo == null || photo.Image == null || photo.Image.Length == 0)
            return NotFound();

        return File(photo.Image, photo.ContentType);
    }

    [HttpGet]
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
