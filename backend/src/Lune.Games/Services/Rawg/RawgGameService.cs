using Lune.Games.Options;
using Lune.Games.Contracts;
using Lune.Games.Models;
using Lune.Shared.Paging;
using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Options;

namespace Lune.Games.Services.Rawg;

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

        var response = await GetAsync<RawgResponse>(url, cancellationToken) ?? new RawgResponse();


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
            var game = await response.Content.ReadFromJsonAsync<RawgGame>(cancellationToken);
            return game is null ? null : ToDetails(game);
        }
        catch (HttpRequestException ex)
        {
            throw new GamesUnavailableException("The game data service could not be reached.", ex);
        }
    }

    /// <summary>
    /// Gets the genre and platform options from RAWG, plus the fixed sort orders.
    /// </summary>
    public async Task<GameFilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken = default)
    {
        var key = Uri.EscapeDataString(RequireKey());
        var genres = await GetAsync<RawgOptionList>($"genres?key={key}", cancellationToken);
        var platforms = await GetAsync<RawgOptionList>($"platforms/lists/parents?key={key}", cancellationToken);

        return new GameFilterOptions(
            (genres?.Results ?? []).Select(g => new FilterOption(g.Slug, g.Name)).ToList(),
            (platforms?.Results ?? []).Select(p => new FilterOption(p.Id.ToString(), p.Name)).ToList(),
            GameOrderings.All);
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken) where T : class
    {
        try
        {
            using var response = await http.GetAsync(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new GamesUnavailableException("The game data service could not be reached.", ex);
        }
    }

    /// <summary>
    /// Searches RAWG developers or publishers by name and returns at most 10 matches.
    /// </summary>
    // public async Task<IReadOnlyList<CompanyOption>> SearchCompaniesAsync(
    /// TODO: implement actual search company method here. 

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
    private static IReadOnlyList<string> Names(List<RawgOption>? items) =>
    items?.Select(i => i.Name).ToList() ?? [];

    /// <summary>
    /// Extracts the platform names from a list of RAWG platform entries.
    /// </summary>
    private static IReadOnlyList<string> PlatformNames(List<RawgPlatformEntry>? items) =>
        items?.Where(p => p.Platform is not null).Select(p => p.Platform!.Name).ToList() ?? [];

    /// <summary>
    /// Converts a RAWG game object to a game summary.
    /// </summary>
    private static GameSummary ToSummary(RawgGame g) => new(
        g.Id, g.Name, g.Released, g.BackgroundImage, g.Rating, g.Metacritic,
        Names(g.Genres), PlatformNames(g.Platforms));

    /// <summary>
    /// Converts a RAWG game object to full game details.
    /// </summary>
    private static GameDetails ToDetails(RawgGame g) => new(
        g.Id, g.Name, g.Released, g.BackgroundImage, g.Rating, g.Metacritic,
        g.DescriptionRaw, g.Website,
        Names(g.Genres), PlatformNames(g.Platforms), Names(g.Developers), Names(g.Publishers));
}
