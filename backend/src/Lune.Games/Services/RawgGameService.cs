using Lune.Games.Options;
using Lune.Games.Contracts;
using Lune.Shared.Paging;
using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Options;

namespace Lune.Games.Services;

internal sealed class RawgGamesService(HttpClient http, IOptions<RawgServiceOptions> options) : IGamesService
{
    private readonly string _apiKey = options.Value.ApiKey;


    /// <summary>
    /// Searches for games using the RAWG API.  
    /// </summary>
    public async Task<PagedResult<GameSummary>> SearchAsync(
        string? search, int page, int pageSize, GameSearchFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"games?key={Uri.EscapeDataString(RequireKey())}&page={page}&page_size={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        var response = await http.GetFromJsonAsync<Rawg.RawgResponse>(url, cancellationToken) ?? new Rawg.RawgResponse(); 


        return new PagedResult<GameSummary>(
            (response.Results?.Select(ToSummary) ?? []).ToList(), page, pageSize, response.Count);
    }
    
    /// <summary>
    /// Gets one game by id, or null when RAWG does not know it.
    /// </summary>
    public async Task<GameDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync(
                $"games/{id}?key={Uri.EscapeDataString(RequireKey())}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var game = await response.Content.ReadFromJsonAsync<Rawg.RawgGame>(cancellationToken);
            return game is null ? null : ToDetails(game);
        }
        catch (HttpRequestException ex)
        {
            throw new GamesUnavailableException("The game data service could not be reached.", ex);
        }
    }

    /// <summary>
    /// Ensures that the RAWG API key is configured and returns it.
    /// </summary>
    private string RequireKey() =>
            string.IsNullOrWhiteSpace(_apiKey)
                ? throw new GamesUnavailableException("The RAWG API key is not configured (Rawg__ApiKey).")
                : _apiKey;
    
    /// <summary>
    /// Extracts the names from a list of RAWG named items.
    /// </summary>
    private static IReadOnlyList<string> Names(List<Rawg.RawgNamed>? items) =>
    items?.Select(i => i.Name).ToList() ?? [];

    /// <summary>
    /// Extracts the platform names from a list of RAWG platform entries.
    /// </summary>
    private static IReadOnlyList<string> PlatformNames(List<Rawg.RawgPlatformEntry>? items) =>
        items?.Where(p => p.Platform is not null).Select(p => p.Platform!.Name).ToList() ?? [];

    /// <summary>
    /// Converts a RAWG game object to a game summary.
    /// </summary>
    private static GameSummary ToSummary(Rawg.RawgGame g) => new(
        g.Id, g.Name, g.Released, g.BackgroundImage, g.Rating, g.Metacritic,
        Names(g.Genres), PlatformNames(g.Platforms));

    /// <summary>
    /// Converts a RAWG game object to full game details.
    /// </summary>
    private static GameDetails ToDetails(Rawg.RawgGame g) => new(
        g.Id, g.Name, g.Released, g.BackgroundImage, g.Rating, g.Metacritic,
        g.DescriptionRaw, g.Website,
        Names(g.Genres), PlatformNames(g.Platforms), Names(g.Developers), Names(g.Publishers));
}
