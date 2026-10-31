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
    

    private string RequireKey() =>
            string.IsNullOrWhiteSpace(_apiKey)
                ? throw new GamesUnavailableException("The RAWG API key is not configured (Rawg__ApiKey).")
                : _apiKey;
    
    private static IReadOnlyList<string> Names(List<Rawg.RawgNamed>? items) =>
    items?.Select(i => i.Name).ToList() ?? [];

    private static IReadOnlyList<string> PlatformNames(List<Rawg.RawgPlatformEntry>? items) =>
        items?.Where(p => p.Platform is not null).Select(p => p.Platform!.Name).ToList() ?? [];

    private static GameSummary ToSummary(Rawg.RawgGame g) => new(
        g.Id, g.Name, g.Released, g.BackgroundImage, g.Rating, g.Metacritic,
        Names(g.Genres), PlatformNames(g.Platforms));
}

