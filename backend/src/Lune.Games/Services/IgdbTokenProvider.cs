using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Lune.Games.Options;
using Microsoft.Extensions.Options;

namespace Lune.Games.Services;

/// <summary>
/// Gets and caches the Twitch app access token that IGDB requires on every request.
/// </summary>
internal sealed class IgdbTokenProvider(IHttpClientFactory httpFactory, IOptions<IgdbServiceOptions> options)
{
    // Refresh a little before the real expiry so a token never expires mid-request.
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    /// <summary>
    /// Returns a valid access token, requesting a new one from Twitch when needed.
    /// </summary>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasValidToken())
        {
            return _token!;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Another request may have refreshed the token while we waited for the lock.
            if (HasValidToken())
            {
                return _token!;
            }

            var opts = options.Value;
            if (string.IsNullOrWhiteSpace(opts.ClientId) || string.IsNullOrWhiteSpace(opts.ApiSecret))
            {
                throw new GamesUnavailableException("The IGDB credentials are not configured (Igdb__ClientId, Igdb__ApiSecret).");
            }

            var url = $"{opts.TokenUrl}?client_id={Uri.EscapeDataString(opts.ClientId)}" +
                      $"&client_secret={Uri.EscapeDataString(opts.ApiSecret)}&grant_type=client_credentials";

            try
            {
                var http = httpFactory.CreateClient();
                using var response = await http.PostAsync(url, null, cancellationToken);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
                if (body is null || string.IsNullOrWhiteSpace(body.AccessToken))
                {
                    throw new GamesUnavailableException("Twitch returned an empty IGDB access token.");
                }

                _token = body.AccessToken;
                _expiresAt = DateTimeOffset.UtcNow.AddSeconds(body.ExpiresIn) - RefreshMargin;
                return _token;
            }
            catch (HttpRequestException ex)
            {
                throw new GamesUnavailableException("Could not get an IGDB access token from Twitch.", ex);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Drops the cached token so the next call fetches a new one (e.g. after IGDB answers 401).
    /// </summary>
    public void Invalidate() => _expiresAt = DateTimeOffset.MinValue;

    private bool HasValidToken() => _token is not null && DateTimeOffset.UtcNow < _expiresAt;

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public long ExpiresIn { get; set; }
    }
}
