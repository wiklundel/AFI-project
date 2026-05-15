
using Google.Cloud.Firestore;

namespace HitsterApp.Services;
public class PlayerService
{
    private readonly FirestoreDb _db;

    public PlayerService(FirestoreService firestoreService)
    {
        _db = firestoreService.Db;
    }

    public async Task AddPlayer(string gameId, string username)
    {
        CollectionReference players =
            _db.Collection("games")
                .Document(gameId)
                .Collection("players");

        var player = new
        {
            Username = username,
            Score = 0
        };

        await players.AddAsync(player);
    }
}