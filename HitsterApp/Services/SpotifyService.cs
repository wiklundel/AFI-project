using System.Text;
using System.Text.Json;
using Google.Cloud.Firestore.V1;
using HitsterApp.Models;

namespace HitsterApp.Services;

public class SpotifyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SpotifyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetAccessToken()
    {
        string clientId = _configuration["Spotify:ClientId"]
            ?? throw new Exception("Missing Spotify ClientId");

        string clientSecret = _configuration["Spotify:ClientSecret"]
            ?? throw new Exception("Missing Spotify ClientSecret");

        string credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")
        );

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Basic {credentials}"
        );

        var body = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });

        var response = await _httpClient.PostAsync(
            "https://accounts.spotify.com/api/token",
            body
        );

        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Spotify token error: " + json);
        }

        using JsonDocument doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("access_token")
            .GetString()
            ?? throw new Exception("Spotify response did not contain access_token");
    }

    /*
    public async Task<MusicCard> GetRandomTrackFromSearch()
    {
        string token = await GetAccessToken();

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        int randomYear = Random.Shared.Next(1980, 2025);

        var response = await _httpClient.GetAsync(
            $"https://api.spotify.com/v1/search?q=year:{randomYear}&type=track&market=SE&limit=50"
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Spotify search error: {json}");
        }

        using JsonDocument doc = JsonDocument.Parse(json);

        var tracks = doc.RootElement
            .GetProperty("tracks")
            .GetProperty("items")
            .EnumerateArray()
            .ToList();

        var track = tracks[Random.Shared.Next(tracks.Count)];

        string title = track.GetProperty("name").GetString() ?? "";

        string artist = track
            .GetProperty("artists")[0]
            .GetProperty("name")
            .GetString() ?? "";

        string releaseDate = track
            .GetProperty("album")
            .GetProperty("release_date")
            .GetString() ?? "0000";

        int releaseYear = int.Parse(releaseDate.Substring(0, 4));

        return new MusicCard
        {
            Title = title,
            Artist = artist,
            ReleaseYear = releaseYear,
            State = "deck"
        };
    }*/

    public async Task<MusicCard> SearchTrack(string search)
    {
        string token = await GetAccessToken();

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {token}"
        );

        var response = await _httpClient.GetAsync(
            $"https://api.spotify.com/v1/search?q={search}&type=track&market=SE&limit=1"
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Spotify search error: " + json);
        }

        using JsonDocument doc = JsonDocument.Parse(json);

        var track = doc.RootElement
            .GetProperty("tracks")
            .GetProperty("items")[0];

        string title =
            track.GetProperty("name").GetString() ?? "";

        string artist =
            track.GetProperty("artists")[0]
                .GetProperty("name")
                .GetString() ?? "";

        string releaseDate =
            track.GetProperty("album")
                .GetProperty("release_date")
                .GetString() ?? "0000";

        int releaseYear =
            int.Parse(releaseDate.Substring(0, 4));

        return new MusicCard
        {
            Title = title,
            Artist = artist,
            ReleaseYear = releaseYear,
            State = "deck"
        };
    }

}