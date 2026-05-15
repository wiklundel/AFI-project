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

		List<MusicCard> cards = new()
		{
			new MusicCard { Title = "Dancing Queen", Artist = "ABBA", ReleaseYear = 1976 },
			new MusicCard { Title = "Billie Jean", Artist = "Michael Jackson", ReleaseYear = 1982 },
			new MusicCard { Title = "Rolling in the Deep", Artist = "Adele", ReleaseYear = 2010 }
		};

		foreach (var card in cards)
		{
			await docRef.Collection("cards").AddAsync(card);
		}

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
		.Select(doc =>
		{
			Player player = doc.ConvertTo<Player>();
			player.PlayerId = doc.Id;
			return player;
		})
		.ToList();

		QuerySnapshot cardsSnapshot = await db
			.Collection("games")
			.Document(id)
			.Collection("cards")
			.GetSnapshotAsync();

		List<MusicCard> cards = cardsSnapshot.Documents
			.Select(doc => doc.ConvertTo<MusicCard>())
			.ToList();

		GameViewModel model = new GameViewModel
		{
			GameId = id,
			Status = game.Status,
			CurrentPlayerId = game.CurrentPlayerId,
			Players = players,
			Cards = cards
		};

		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> DrawCard(string gameId)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "serviceAccountKey.json");

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		CollectionReference cardsRef = db
			.Collection("games")
			.Document(gameId)
			.Collection("cards");

		// 1. Kolla om det redan finns ett pending-kort
		QuerySnapshot pendingCards = await cardsRef
			.WhereEqualTo("State", "pending")
			.Limit(1)
			.GetSnapshotAsync();

		if (pendingCards.Documents.Count > 0)
		{
			return RedirectToAction("Game", new { id = gameId });
		}

		// 2. Om inget pending finns, dra ett nytt deck-kort
		QuerySnapshot deckCards = await cardsRef
			.WhereEqualTo("State", "deck")
			.Limit(1)
			.GetSnapshotAsync();

		if (deckCards.Documents.Count > 0)
		{
			DocumentSnapshot card = deckCards.Documents[0];
			await card.Reference.UpdateAsync("State", "pending");
		}

		return RedirectToAction("Game", new { id = gameId });
	}

	[HttpPost]
	public async Task<IActionResult> GuessCorrect(string gameId)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "serviceAccountKey.json");

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		QuerySnapshot pendingCards = await db
			.Collection("games")
			.Document(gameId)
			.Collection("cards")
			.WhereEqualTo("State", "pending")
			.Limit(1)
			.GetSnapshotAsync();

		DocumentReference gameRef = db.Collection("games").Document(gameId);
		DocumentSnapshot gameSnapshot = await gameRef.GetSnapshotAsync();
		Game game = gameSnapshot.ConvertTo<Game>();

		if (pendingCards.Documents.Count > 0)
		{
			await pendingCards.Documents[0].Reference.UpdateAsync(new Dictionary<string, object>
			{
				{ "State", "guessed" },
				{ "PlayerId", game.CurrentPlayerId }
			});
		}

		return RedirectToAction("Game", new { id = gameId });
	}

	[HttpPost]
	public async Task<IActionResult> GuessWrong(string gameId)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "serviceAccountKey.json");

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		QuerySnapshot pendingCards = await db
			.Collection("games")
			.Document(gameId)
			.Collection("cards")
			.WhereEqualTo("State", "pending")
			.Limit(1)
			.GetSnapshotAsync();

		if (pendingCards.Documents.Count > 0)
		{
			await pendingCards.Documents[0].Reference.UpdateAsync("State", "discarded");
		}

		return RedirectToAction("Game", new { id = gameId });
	}

	[HttpPost]
	public async Task<IActionResult> EndTurn(string gameId)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "serviceAccountKey.json");

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		DocumentReference gameRef = db.Collection("games").Document(gameId);
		DocumentSnapshot gameSnapshot = await gameRef.GetSnapshotAsync();
		Game game = gameSnapshot.ConvertTo<Game>();

		CollectionReference cardsRef = gameRef.Collection("cards");

		QuerySnapshot guessedCards = await cardsRef
			.WhereEqualTo("State", "guessed")
			.WhereEqualTo("PlayerId", game.CurrentPlayerId)
			.GetSnapshotAsync();

		foreach (DocumentSnapshot card in guessedCards.Documents)
		{
			await card.Reference.UpdateAsync("State", "safe");
		}

		QuerySnapshot playersSnapshot = await gameRef.Collection("players").GetSnapshotAsync();

		List<string> playerIds = playersSnapshot.Documents
			.Select(doc => doc.Id)
			.ToList();

		int currentIndex = playerIds.IndexOf(game.CurrentPlayerId);
		int nextIndex = (currentIndex + 1) % playerIds.Count;
		string nextPlayerId = playerIds[nextIndex];

		await gameRef.UpdateAsync("CurrentPlayerId", nextPlayerId);

		return RedirectToAction("Game", new { id = gameId });
	}
}
