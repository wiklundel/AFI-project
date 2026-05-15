using HitsterApp.Models;
using HitsterApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace HitsterApp.Controllers;

public class SpotifyController : Controller
{
    private readonly SpotifyService _spotifyService;

    public SpotifyController(SpotifyService spotifyService)
    {
        _spotifyService = spotifyService;
    }

    public async Task<IActionResult> Test()
    {
        string token = await _spotifyService.GetAccessToken();

        return Content(token);
    }

    public async Task<IActionResult> RandomCard()
    {
        string playlistId = "37i9dQZF1DXcBWIGoYBM5M";

        MusicCard card = await _spotifyService.GetRandomTrackFromPlaylist(playlistId);

        return Json(card);
    }
}