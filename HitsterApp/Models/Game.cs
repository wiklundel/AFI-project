namespace HitsterApp.Models;

public class Game
{
    public string Status { get; set; } = "waiting";
    public string CurrentPlayerId { get; set; } = "";
    public string WinnerId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}