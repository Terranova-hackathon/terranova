using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Lune.Games.Contracts;
using Lune.Games.Igdb;
using Lune.Games.Options;
using Lune.Shared.Paging;
using Microsoft.Extensions.Options;

namespace Lune.Games.Services;

internal sealed class IgdbGamesService(
    HttpClient http, IOptions<IgdbServiceOptions> options, IgdbTokenProvider tokens) : IGamesService
{
    private const string SummaryFields =
        "id,name,first_release_date,cover.image_id,total_rating,aggregated_rating,genres.name,platforms.name";

    private const string DetailFields =
        SummaryFields + ",summary,url,involved_companies.developer,involved_companies.publisher,involved_companies.company.name";

    private readonly IgdbServiceOptions _options = options.Value;

    /// <summary>
    /// Searches for games using the IGDB API. Without a search term the most rated games are listed.
    /// </summary>
    public async Task<PagedResult<GameSummary>> SearchAsync(
        string? search, int page, int pageSize, GameSearchFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var offset = Math.Max(page - 1, 0) * pageSize;

        // IGDB does not allow "sort" together with "search", so the default list is the only one that sorts.
        var criteria = string.IsNullOrWhiteSpace(search)
            ? "where total_rating_count > 0;"
            : $"search \"{Escape(search.Trim())}\";";
        var order = string.IsNullOrWhiteSpace(search) ? " sort total_rating_count desc;" : string.Empty;

        var games = await PostAsync<List<IgdbGame>>(
            "games", $"fields {SummaryFields}; {criteria}{order} limit {pageSize}; offset {offset};", cancellationToken);
        var count = await PostAsync<IgdbCount>("games/count", criteria, cancellationToken);

        return new PagedResult<GameSummary>(
            (games?.Select(ToSummary) ?? []).ToList(), page, pageSize, count?.Count ?? 0);
    }

    /// <summary>
    /// Gets one game by id, or null when IGDB does not know it.
    /// </summary>
    public async Task<GameDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var games = await PostAsync<List<IgdbGame>>(
            "games", $"fields {DetailFields}; where id = {id}; limit 1;", cancellationToken);

        var game = games?.FirstOrDefault();
        return game is null ? null : ToDetails(game);
    }

    /// <summary>
    /// Sends an IGDB query (Apicalypse syntax) and reads the JSON answer.
    /// Retries once with a fresh token when IGDB rejects the cached one.
    /// </summary>
    private async Task<T?> PostAsync<T>(string endpoint, string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new GamesUnavailableException("The IGDB client id is not configured (Igdb__ClientId).");
        }

        try
        {
            for (var attempt = 0; ; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(query, Encoding.UTF8, "text/plain")
                };
                request.Headers.Add("Client-ID", _options.ClientId);
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", await tokens.GetTokenAsync(cancellationToken));

                using var response = await http.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 0)
                {
                    tokens.Invalidate();
                    continue;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new GamesUnavailableException("The game data service could not be reached.", ex);
        }
    }

    /// <summary>
    /// Escapes text so it can sit inside a quoted IGDB string.
    /// </summary>
    private static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>
    /// Extracts the names from a list of IGDB named items.
    /// </summary>
    private static IReadOnlyList<string> Names(List<IgdbNamed>? items) =>
        items?.Select(i => i.Name).ToList() ?? [];

    /// <summary>
    /// Extracts the names of the companies that have the given role (developer or publisher).
    /// </summary>
    private static IReadOnlyList<string> Companies(List<IgdbInvolvedCompany>? items, Func<IgdbInvolvedCompany, bool> role) =>
        items?.Where(c => role(c) && c.Company is not null).Select(c => c.Company!.Name).Distinct().ToList() ?? [];

    /// <summary>
    /// Converts IGDB's Unix timestamp to the YYYY-MM-DD string the API contract uses.
    /// </summary>
    private static string? ToReleaseDate(long? unixSeconds) =>
        unixSeconds is null
            ? null
            : DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// Builds the cover URL from IGDB's image id.
    /// </summary>
    private string? ToImageUrl(IgdbCover? cover) =>
        string.IsNullOrWhiteSpace(cover?.ImageId)
            ? null
            : $"{_options.ImageBaseUrl.TrimEnd('/')}/t_720p/{cover.ImageId}.jpg";

    /// <summary>
    /// Converts IGDB's 0-100 rating to the 0-5 scale the API contract uses.
    /// </summary>
    private static double ToRating(double? totalRating) =>
        totalRating is null ? 0 : Math.Round(totalRating.Value / 20, 2);

    /// <summary>
    /// IGDB has no Metacritic score, so the critic rating (0-100) stands in for it.
    /// </summary>
    private static int? ToMetacritic(double? aggregatedRating) =>
        aggregatedRating is null ? null : (int)Math.Round(aggregatedRating.Value);

    /// <summary>
    /// Converts an IGDB game object to a game summary.
    /// </summary>
    private GameSummary ToSummary(IgdbGame g) => new(
        g.Id, g.Name, ToReleaseDate(g.FirstReleaseDate), ToImageUrl(g.Cover), ToRating(g.TotalRating),
        ToMetacritic(g.AggregatedRating), Names(g.Genres), Names(g.Platforms));

    /// <summary>
    /// Converts an IGDB game object to full game details.
    /// </summary>
    private GameDetails ToDetails(IgdbGame g) => new(
        g.Id, g.Name, ToReleaseDate(g.FirstReleaseDate), ToImageUrl(g.Cover), ToRating(g.TotalRating),
        ToMetacritic(g.AggregatedRating), g.Summary, g.Url,
        Names(g.Genres), Names(g.Platforms),
        Companies(g.InvolvedCompanies, c => c.Developer), Companies(g.InvolvedCompanies, c => c.Publisher));
}
