using Google.Cloud.Firestore;

namespace HitsterApp.Models;

[FirestoreData]
public class Game
{
    [FirestoreProperty]
    public string Status { get; set; } = "waiting";

    [FirestoreProperty]
    public string CurrentPlayerId { get; set; } = "";

    [FirestoreProperty]
    public string WinnerId { get; set; } = "";

    [FirestoreProperty]
    public Timestamp CreatedAt { get; set; } = Timestamp.GetCurrentTimestamp();
}