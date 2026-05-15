using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HitsterApp.Models;

namespace HitsterApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

	[HttpPost]
	public IActionResult StartGame()
	{
		// Tillfälligt gameId tills Firebase-kopplingen är klar
		string gameId = Guid.NewGuid().ToString();

		return RedirectToAction("Game", new { id = gameId });
	}

	[HttpGet]
	public IActionResult Game(string id)
	{
		ViewBag.GameId = id;
		return View();
	}
}
