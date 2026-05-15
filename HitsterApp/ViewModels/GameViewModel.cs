using HitsterApp.Models;

namespace HitsterApp.ViewModels;

public class GameViewModel
{
    public string GameId { get; set; } = "";
    public string Status { get; set; } = "";
    public string CurrentPlayerId { get; set; } = "";
    public List<Player> Players { get; set; } = new();
	public List<MusicCard> Cards { get; set; } = new();
}