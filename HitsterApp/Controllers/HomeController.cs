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

		List<DocumentReference> playerRefs = new()
		{
			player1Ref,
			player2Ref
		};

		await docRef.UpdateAsync("CurrentPlayerId", player1Ref.Id);

		List<MusicCard> cards = new()
		{
			new MusicCard { Title = "Rolling in the Deep", Artist = "Adele", ReleaseYear = 2010 },
			new MusicCard { Title = "Dancing Queen", Artist = "ABBA", ReleaseYear = 1976 },
			new MusicCard { Title = "Billie Jean", Artist = "Michael Jackson", ReleaseYear = 1982 },
			new MusicCard { Title = "Wonderwall", Artist = "Oasis", ReleaseYear = 1995 },
			new MusicCard { Title = "Poker Face", Artist = "Lady Gaga", ReleaseYear = 2008 },
			new MusicCard { Title = "Blinding Lights", Artist = "The Weeknd", ReleaseYear = 2019 },
			new MusicCard { Title = "Hey Jude", Artist = "The Beatles", ReleaseYear = 1968 },
			new MusicCard { Title = "Rolling in the Deep", Artist = "Adele", ReleaseYear = 2010 },
			new MusicCard { Title = "Take On Me", Artist = "A-ha", ReleaseYear = 1985 },
			new MusicCard { Title = "Lose Yourself", Artist = "Eminem", ReleaseYear = 2002 },
			new MusicCard { Title = "Bad Romance", Artist = "Lady Gaga", ReleaseYear = 2009 },
			new MusicCard { Title = "Titanium", Artist = "David Guetta", ReleaseYear = 2011 },
			new MusicCard { Title = "Smells Like Teen Spirit", Artist = "Nirvana", ReleaseYear = 1991 },
			new MusicCard { Title = "Someone Like You", Artist = "Adele", ReleaseYear = 2011 },
			new MusicCard { Title = "Wake Me Up", Artist = "Avicii", ReleaseYear = 2013 },
			new MusicCard { Title = "Bohemian Rhapsody", Artist = "Queen", ReleaseYear = 1975 }
		};

		Random random = new Random();

		cards = cards
			.OrderBy(c => random.Next())
			.ToList();

		for (int i = 0; i < cards.Count; i++)
		{
			if (i < playerRefs.Count)
			{
				cards[i].PlayerId = playerRefs[i].Id;
				cards[i].State = "safe";
			}

			await docRef.Collection("cards").AddAsync(cards[i]);
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

		string currentPlayerName = players
			.FirstOrDefault(p => p.PlayerId == game.CurrentPlayerId)?
			.Username ?? "Okänd spelare";

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
			CurrentPlayerName = currentPlayerName,
			Players = players,
			Cards = cards,
			WinnerId = game.WinnerId
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

		// 2. Hämta alla kort som finns kvar i deck
		QuerySnapshot deckCards = await cardsRef
			.WhereEqualTo("State", "deck")
			.GetSnapshotAsync();

		if (deckCards.Documents.Count == 0)
		{
			return RedirectToAction("Game", new { id = gameId });
		}

		// 3. Välj ett slumpmässigt kort
		Random random = new Random();

		DocumentSnapshot randomCard = deckCards.Documents[
			random.Next(deckCards.Documents.Count)
		];

		await randomCard.Reference.UpdateAsync("State", "pending");

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
		string path = Path.Combine(
			Directory.GetCurrentDirectory(),
			"json",
			"serviceAccountKey.json"
		);

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		DocumentReference gameRef = db.Collection("games").Document(gameId);
		DocumentSnapshot gameSnapshot = await gameRef.GetSnapshotAsync();
		Game game = gameSnapshot.ConvertTo<Game>();

		CollectionReference cardsRef = gameRef.Collection("cards");

		// 1. Släng pending-kortet
		QuerySnapshot pendingCards = await cardsRef
			.WhereEqualTo("State", "pending")
			.Limit(1)
			.GetSnapshotAsync();

		foreach (DocumentSnapshot card in pendingCards.Documents)
		{
			await card.Reference.UpdateAsync("State", "discarded");
		}

		// 2. Släng spelarens osäkrade kort
		QuerySnapshot guessedCards = await cardsRef
			.WhereEqualTo("State", "guessed")
			.WhereEqualTo("PlayerId", game.CurrentPlayerId)
			.GetSnapshotAsync();

		foreach (DocumentSnapshot card in guessedCards.Documents)
		{
			await card.Reference.UpdateAsync("State", "discarded");
		}

		// 3. Byt tur
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

		QuerySnapshot safeCards = await cardsRef
		.WhereEqualTo("State", "safe")
		.WhereEqualTo("PlayerId", game.CurrentPlayerId)
		.GetSnapshotAsync();

		int newScore = safeCards.Documents.Count;

		DocumentReference currentPlayerRef = gameRef
			.Collection("players")
			.Document(game.CurrentPlayerId);

		await currentPlayerRef.UpdateAsync("Score", newScore);

		if (newScore >= 10)
		{
			await gameRef.UpdateAsync(new Dictionary<string, object>
			{
				{ "Status", "finished" },
				{ "WinnerId", game.CurrentPlayerId }
			});

			return RedirectToAction("Game", new { id = gameId });
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

	[HttpPost]
	public async Task<IActionResult> PlaceCard(string gameId, int position, string guessedTitle, string guessedArtist)
	{
		string path = Path.Combine(
			Directory.GetCurrentDirectory(),
			"json",
			"serviceAccountKey.json"
		);

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		DocumentReference gameRef = db.Collection("games").Document(gameId);
		DocumentSnapshot gameSnapshot = await gameRef.GetSnapshotAsync();
		Game game = gameSnapshot.ConvertTo<Game>();

		CollectionReference cardsRef = gameRef.Collection("cards");

		QuerySnapshot pendingSnapshot = await cardsRef
			.WhereEqualTo("State", "pending")
			.Limit(1)
			.GetSnapshotAsync();

		if (pendingSnapshot.Documents.Count == 0)
		{
			return RedirectToAction("Game", new { id = gameId });
		}

		DocumentSnapshot pendingDoc = pendingSnapshot.Documents[0];
		MusicCard pendingCard = pendingDoc.ConvertTo<MusicCard>();

		QuerySnapshot timelineSnapshot = await cardsRef
			.WhereEqualTo("PlayerId", game.CurrentPlayerId)
			.GetSnapshotAsync();

		List<MusicCard> timeline = timelineSnapshot.Documents
			.Select(doc => doc.ConvertTo<MusicCard>())
			.Where(c => c.State == "safe" || c.State == "guessed")
			.OrderBy(c => c.ReleaseYear)
			.ToList();

		bool correctPlacement = true;

		if (position > 0)
		{
			int leftYear = timeline[position - 1].ReleaseYear;

			if (pendingCard.ReleaseYear < leftYear)
			{
				correctPlacement = false;
			}
		}

		if (position < timeline.Count)
		{
			int rightYear = timeline[position].ReleaseYear;

			if (pendingCard.ReleaseYear > rightYear)
			{
				correctPlacement = false;
			}
		}

		if (correctPlacement)
		{
			await pendingDoc.Reference.UpdateAsync(new Dictionary<string, object>
			{
				{ "State", "guessed" },
				{ "PlayerId", game.CurrentPlayerId }
			});

			bool titleCorrect = IsCloseEnough(pendingCard.Title, guessedTitle);
			bool artistCorrect = IsCloseEnough(pendingCard.Artist, guessedArtist);

			if (titleCorrect && artistCorrect)
			{
				DocumentReference playerRef = gameRef
					.Collection("players")
					.Document(game.CurrentPlayerId);

				DocumentSnapshot playerSnapshot = await playerRef.GetSnapshotAsync();
				Player player = playerSnapshot.ConvertTo<Player>();

				await playerRef.UpdateAsync("Tokens", player.Tokens + 1);
			}
		}
		else
		{
			await pendingDoc.Reference.UpdateAsync("State", "discarded");

			QuerySnapshot guessedCards = await cardsRef
				.WhereEqualTo("State", "guessed")
				.WhereEqualTo("PlayerId", game.CurrentPlayerId)
				.GetSnapshotAsync();

			foreach (DocumentSnapshot card in guessedCards.Documents)
			{
				await card.Reference.UpdateAsync("State", "discarded");
			}

			QuerySnapshot playersSnapshot = await gameRef.Collection("players").GetSnapshotAsync();

			List<string> playerIds = playersSnapshot.Documents
				.Select(doc => doc.Id)
				.ToList();

			int currentIndex = playerIds.IndexOf(game.CurrentPlayerId);
			int nextIndex = (currentIndex + 1) % playerIds.Count;
			string nextPlayerId = playerIds[nextIndex];

			await gameRef.UpdateAsync("CurrentPlayerId", nextPlayerId);
		}

		return RedirectToAction("Game", new { id = gameId });
	}

	[HttpPost]
	public async Task<IActionResult> SkipCard(string gameId)
	{
		string path = Path.Combine(
			Directory.GetCurrentDirectory(),
			"json",
			"serviceAccountKey.json"
		);

		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

		FirestoreDb db = FirestoreDb.Create("hitsterapp-1902d");

		DocumentReference gameRef = db.Collection("games").Document(gameId);
		DocumentSnapshot gameSnapshot = await gameRef.GetSnapshotAsync();
		Game game = gameSnapshot.ConvertTo<Game>();

		DocumentReference playerRef = gameRef
			.Collection("players")
			.Document(game.CurrentPlayerId);

		DocumentSnapshot playerSnapshot = await playerRef.GetSnapshotAsync();

		if (!playerSnapshot.Exists)
		{
			return RedirectToAction("Game", new { id = gameId });
		}

		Player player = playerSnapshot.ConvertTo<Player>();

		if (player.Tokens < 1)
		{
			return RedirectToAction("Game", new { id = gameId });
		}

		CollectionReference cardsRef = gameRef.Collection("cards");

		QuerySnapshot pendingSnapshot = await cardsRef
			.WhereEqualTo("State", "pending")
			.Limit(1)
			.GetSnapshotAsync();

		if (pendingSnapshot.Documents.Count == 0)
		{
			return RedirectToAction("Game", new { id = gameId });
		}

		await playerRef.UpdateAsync("Tokens", player.Tokens - 1);

		DocumentSnapshot oldCard = pendingSnapshot.Documents[0];
		await oldCard.Reference.UpdateAsync("State", "discarded");

		QuerySnapshot deckSnapshot = await cardsRef
			.WhereEqualTo("State", "deck")
			.GetSnapshotAsync();

		if (deckSnapshot.Documents.Count > 0)
		{
			Random random = new Random();

			DocumentSnapshot newCard = deckSnapshot.Documents[
				random.Next(deckSnapshot.Documents.Count)
			];

			await newCard.Reference.UpdateAsync("State", "pending");
		}

		return RedirectToAction("Game", new { id = gameId });
	}

		private static string NormalizeGuess(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return "";

		text = text.ToLower().Trim();

		// Ta bort features i parentes: "(ft. ...)", "(feat. ...)", "(featuring ...)"
		text = System.Text.RegularExpressions.Regex.Replace(
			text,
			@"\((feat\.?|ft\.?|featuring).*?\)",
			""
		);

		// Ta bort features efter bindestreck: "- feat. ..."
		text = System.Text.RegularExpressions.Regex.Replace(
			text,
			@"[-–—]\s*(feat\.?|ft\.?|featuring).*",
			""
		);

		// Ta bort onödiga tecken
		text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9åäö ]", "");

		// Ta bort extra mellanslag
		text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

		return text;
	}

	private static int LevenshteinDistance(string a, string b)
	{
		int[,] dp = new int[a.Length + 1, b.Length + 1];

		for (int i = 0; i <= a.Length; i++)
			dp[i, 0] = i;

		for (int j = 0; j <= b.Length; j++)
			dp[0, j] = j;

		for (int i = 1; i <= a.Length; i++)
		{
			for (int j = 1; j <= b.Length; j++)
			{
				int cost = a[i - 1] == b[j - 1] ? 0 : 1;

				dp[i, j] = Math.Min(
					Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
					dp[i - 1, j - 1] + cost
				);
			}
		}

		return dp[a.Length, b.Length];
	}

	private static bool IsCloseEnough(string correct, string guess)
	{
		correct = NormalizeGuess(correct);
		guess = NormalizeGuess(guess);

		if (string.IsNullOrWhiteSpace(guess))
			return false;

		if (correct == guess)
			return true;

		int distance = LevenshteinDistance(correct, guess);

		// Tillåt fler småfel på längre titlar
		int allowedErrors = correct.Length switch
		{
			<= 5 => 0,
			<= 10 => 1,
			<= 20 => 2,
			_ => 3
		};

		return distance <= allowedErrors;
	}
}
