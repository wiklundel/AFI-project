using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HitsterApp.Models;
using Google.Cloud.Firestore;
using HitsterApp.Models;
using Google.Apis.Auth.OAuth2;

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
	public async Task<IActionResult> StartGame()
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "serviceAccountKey.json");

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
		
		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		Game game = new Game();

		DocumentReference docRef = await db.Collection("games").AddAsync(game);

		return RedirectToAction("Game", new { id = docRef.Id });
	}

	[HttpGet]
	public async Task<IActionResult> Game(string id)
	{
		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		DocumentSnapshot snapshot = await db.Collection("games").Document(id).GetSnapshotAsync();

		ViewBag.GameId = id;
		ViewBag.Status = snapshot.GetValue<string>("Status");

		return View();
	}
}
