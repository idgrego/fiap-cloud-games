using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.domain.entities;
using fase_01.application.interfaces;
using fase_01.application.dtos;
using fase_01.application.mappings;

namespace fase_01.Controllers;

public class UserController : Controller
{

    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;

    public UserController(IUserRepository userRepository, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var entities = await _userRepository.ListAllAsync();
        var dtos = entities.Select(u => u.ToDto());
        return View(dtos);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var entity = await _userRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity.ToDto());
    }

    #region create item

    [HttpGet]
    public IActionResult Create()
    {
        return View(new UserDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserDto dto)
    {
        if (ModelState.IsValid)
        {
            var entity = dto.ToEntity();
            await _userRepository.AddAsync(entity);

            if (dto.Photo != null && dto.Photo.Length > 0)
                await _photoService.SaveUserPhotoAsync(entity.Id, dto.Photo);

            return RedirectToAction(nameof(Index));
        }

        return View(dto);
    }

    #endregion

    #region update item

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var entity = await _userRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity.ToDto());
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UserDto dto)
    {

        if (ModelState.IsValid)
        {
            var existingEntity = await _userRepository.GetByIdAsync(dto.Id);
            if (existingEntity == null) return NotFound();

            var entity = dto.ToEntity(existingEntity);
            await _userRepository.UpdateAsync(entity);

            if (dto.Photo != null && dto.Photo.Length > 0)
                await _photoService.SaveUserPhotoAsync(entity.Id, dto.Photo);

            return RedirectToAction(nameof(Index));
        }

        return View(dto);
    }

    #endregion

    #region delete item

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _userRepository.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity.ToDto());
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _userRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    #endregion


    #region photos

    [HttpGet]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _photoService.GetUserPhotoAsync(id);
        if (photo == null || photo.Image == null || photo.Image.Length == 0)
            return NotFound();

        return File(photo.Image, photo.ContentType);
    }

    [HttpGet]
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
