using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.domain.entities;

namespace fase_01.Controllers;

public class UserController : Controller
{

    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.ListAllAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return View(user);
    }

    #region create item

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            await _userRepository.AddAsync(user);
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    #endregion

    #region update item

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var item = await _userRepository.GetByIdAsync(id);
        return View(item);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(User user)
    {
        if (ModelState.IsValid)
        {
            await _userRepository.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    #endregion

    #region delete item

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _userRepository.GetByIdAsync(id);
        return View(item);
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExecute(int id)
    {
        var item = await _userRepository.GetByIdAsync(id);

        if (item == null)
            // o item não existe, redireciona para a página de listagem
            return RedirectToAction(nameof(Index));

        if (ModelState.IsValid)
        {
            await _userRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        return View(item);
    }

    #endregion

}
