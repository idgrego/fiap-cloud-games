using Microsoft.AspNetCore.Mvc;
using fase_01.domain.interfaces;
using fase_01.domain.entities;

namespace fase_01.Controllers;

public class GameController : Controller
{

    private readonly IGameRepository _gameRepository;

    public GameController(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var games = await _gameRepository.ListAllAsync();
        return View(games);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        return View(game);
    }

    #region create item

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Game game)
    {
        if (ModelState.IsValid)
        {
            await _gameRepository.AddAsync(game);
            return RedirectToAction(nameof(Index));
        }
        return View(game);
    }

    #endregion

    #region update item

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var item = await _gameRepository.GetByIdAsync(id);
        return View(item);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Game game)
    {
        if (ModelState.IsValid)
        {
            await _gameRepository.UpdateAsync(game);
            return RedirectToAction(nameof(Index));
        }
        return View(game);
    }

    #endregion

    #region delete item

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _gameRepository.GetByIdAsync(id);
        return View(item);
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExecute(int id)
    {
        var item = await _gameRepository.GetByIdAsync(id);

        if (item == null)
            // o item não existe, redireciona para a página de listagem
            return RedirectToAction(nameof(Index));

        if (ModelState.IsValid)
        {
            await _gameRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        return View(item);
    }

    #endregion

}
