using Google.Cloud.Firestore;
using HitsterApp.Models;

namespace HitsterApp.Services;

public class MusicCardService
{
    private readonly FirestoreDb _db;

    public MusicCardService(
        FirestoreService firestoreService)
    {
        _db = firestoreService.Db;
    }

    public async Task AddCard(
        string gameId,
        MusicCard card)
    {
        await _db
            .Collection("games")
            .Document(gameId)
            .Collection("musicCards")
            .AddAsync(card);
    }
}