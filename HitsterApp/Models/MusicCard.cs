using Google.Cloud.Firestore;

namespace HitsterApp.Models;

[FirestoreData]
public class MusicCard
{
    [FirestoreProperty]
    public string Title { get; set; } = "";

    [FirestoreProperty]
    public string Artist { get; set; } = "";

    [FirestoreProperty]
    public int ReleaseYear { get; set; }

    [FirestoreProperty]
    public string SpotifyId { get; set; } = "";

    [FirestoreProperty]
    public string PreviewUrl { get; set; } = "";

    [FirestoreProperty]
    public string State { get; set; } = "deck";

    [FirestoreProperty]
    public string PlayerId { get; set; } = "";
}