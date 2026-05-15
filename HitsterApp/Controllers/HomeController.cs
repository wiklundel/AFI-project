using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HitsterApp.Models;
using Google.Cloud.Firestore;
using HitsterApp.Models;
using Google.Apis.Auth.OAuth2;
using HitsterApp.ViewModels;

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

		Player player1 = new Player
		{
			Username = "Anna"
		};

		Player player2 = new Player
		{
			Username = "Elvira"
		};

		DocumentReference player1Ref = await docRef.Collection("players").AddAsync(player1);
		DocumentReference player2Ref = await docRef.Collection("players").AddAsync(player2);

		await docRef.UpdateAsync("CurrentPlayerId", player1Ref.Id);

		return RedirectToAction("Game", new { id = docRef.Id });
	}

	[HttpGet]
	public async Task<IActionResult> Game(string id)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "serviceAccountKey.json");

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		DocumentSnapshot gameSnapshot = await db.Collection("games").Document(id).GetSnapshotAsync();

		Game game = gameSnapshot.ConvertTo<Game>();

		QuerySnapshot playersSnapshot = await db
			.Collection("games")
			.Document(id)
			.Collection("players")
			.GetSnapshotAsync();

		List<Player> players = playersSnapshot.Documents
			.Select(doc => doc.ConvertTo<Player>())
			.ToList();

		GameViewModel model = new GameViewModel
		{
			GameId = id,
			Status = game.Status,
			CurrentPlayerId = game.CurrentPlayerId,
			Players = players
		};

		return View(model);
	}
}
