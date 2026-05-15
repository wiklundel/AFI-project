namespace HitsterApp.Models;

public class MusicCard
{
    public string cardId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public int ReleaseYear { get; set; }
    public string? State { get; set; }
}