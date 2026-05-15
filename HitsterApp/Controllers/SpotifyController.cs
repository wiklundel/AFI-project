using HitsterApp.Models;
using HitsterApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace HitsterApp.Controllers;

public class SpotifyController : Controller
{
    private readonly SpotifyService _spotifyService;
    private readonly MusicCardService _musicCardService;

    public SpotifyController(SpotifyService spotifyService, MusicCardService musicCardService)
    {
        _spotifyService = spotifyService;
        _musicCardService = musicCardService;
    }

    public async Task<IActionResult> Test()
    {
        string token = await _spotifyService.GetAccessToken();

        return Content(token);
    }

    public async Task<IActionResult> Search(string q)
    {
        MusicCard card = await _spotifyService.SearchTrack(q);

        string gameId = "game1";

        await _musicCardService.AddCard(gameId, card);
        // MusicCard card = await _spotifyService.GetRandomTrackFromSearch();

        return Json(card);
    }
}