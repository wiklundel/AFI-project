using HitsterApp.Models;
using HitsterApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace HitsterApp.Controllers;

public class SpotifyController : Controller
{
    private readonly SpotifyService _spotifyService;
    private readonly MusicCardService _musicCardService;
    private readonly IConfiguration _configuration;

    public SpotifyController(SpotifyService spotifyService, MusicCardService musicCardService, IConfiguration configuration)
    {
        _spotifyService = spotifyService;
        _musicCardService = musicCardService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Test()
    {
        string token = await _spotifyService.GetAccessToken();

        return Content(token);
    }

    public IActionResult Login()
    {
        string clientId = _configuration["Spotify:ClientId"!];
        string redirectUri = "http://130.239.190.149:5290/Spotify/Callback";

        string scopes = "playlist-read-private playlist-read-collaborative";

        string url = 
            "https://accounts.spotify.com/authorize" +
            "?response_type=code" +
            $"&client_id={clientId}" +
            $"&scope={Uri.EscapeDataString(scopes)}" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}";

        return Redirect(url);
    }

    public async Task<IActionResult> ImportPlaylist(string playlistId)
    {
        string? accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

        if (accessToken == null)
        {
            return RedirectToAction("Login");
        }

        List<MusicCard> cards = await _spotifyService.GetPlaylistTracks(accessToken, playlistId);

        return Json(cards);
    }

    public async Task<IActionResult> Callback(string code)
    {
        string accessToken = await _spotifyService.GetUserAccessToken(code);

        HttpContext.Session.SetString("SpotifyAccessToken", accessToken);

        return RedirectToAction("ImportPlaylist");
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