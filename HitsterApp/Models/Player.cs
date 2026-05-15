using Google.Cloud.Firestore;

namespace HitsterApp.Models;

[FirestoreData]
public class Player
{
    public string PlayerId { get; set; } = "";
	
	[FirestoreProperty]
    public string Username { get; set; } = "";

    [FirestoreProperty]
    public int Score { get; set; } = 1;

    [FirestoreProperty]
    public int Tokens { get; set; } = 2;

    [FirestoreProperty]
    public List<string> Timeline { get; set; } = new();
}