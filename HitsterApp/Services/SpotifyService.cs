using System.Text;
using System.Text.Json;
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
        string clientId = _configuration["Spotify:ClientId"];
        string clientSecret = _configuration["Spotify:ClientSecret"];

        string credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{clientId}:{clientSecret}"
            )
        );

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Basic {credentials}"
        );

        var body = 
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                });


        var response = await _httpClient.PostAsync(
            "https://accounts.spotify.com/api/token",
            body
        );
        
        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();
        
        using JsonDocument doc = JsonDocument.Parse(json);

        return doc.RootElement
                  .GetProperty("access_token")
                  .GetString();
    }
}