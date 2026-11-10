using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Lune.Games.Contracts;
using Lune.Games.Igdb;
using Lune.Games.Options;
using Lune.Shared.Paging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Lune.Games.Services;

internal sealed class IgdbGamesService(
    HttpClient http, IOptions<IgdbServiceOptions> options, IgdbTokenProvider tokens, IMemoryCache cache) : IGamesService
{
    private readonly IgdbServiceOptions _options = options.Value;

    /// <summary>
    /// Searches for games using the IGDB API. Without a search term the most rated games are listed.
    /// </summary>
    public async Task<PagedResult<GameSummary>> SearchAsync(
        string? search, int page, int pageSize, GameSearchFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var offset = Math.Max(page - 1, 0) * pageSize;
        var criteria = BuildCriteria(search, filters);
        var order = BuildOrdering(search, filters);

        var games = await PostAsync<List<IgdbGame>>(
            "games", $"fields {IgdbGameFields.Summary}; {criteria}{order} limit {pageSize}; offset {offset};", cancellationToken);
        var count = await PostAsync<IgdbCount>("games/count", criteria, cancellationToken);
 
        return new PagedResult<GameSummary>(
            (games?.Select(ToSummary) ?? []).ToList(), page, pageSize, count?.Count ?? 0);
    }

    //Converts the out search params into a IGDB readale parameter. 
    private static string BuildCriteria(string? search, GameSearchFilters? filters)
    {
        var conditions = new List<string> { "first_release_date != null", "total_rating != null" };
        if (filters is not null)
        {
            if (filters.Genres.Count > 0)
                conditions.Add($"genres.slug = ({QuotedValues(filters.Genres)})");

            if (filters.Platforms.Count > 0)
                conditions.Add($"platforms = ({string.Join(",", filters.Platforms)})");

            if (filters.Developers.Count > 0)
            {
                conditions.Add(
                    $"involved_companies.developer = true & involved_companies.company.slug = ({QuotedValues(filters.Developers)})");
            }

            if (filters.Publishers.Count > 0)
            {
                conditions.Add(
                    $"involved_companies.publisher = true & involved_companies.company.slug = ({QuotedValues(filters.Publishers)})");
            }

            if (filters.MetacriticMin is not null)
                conditions.Add($"aggregated_rating >= {filters.MetacriticMin.Value}");

            if (filters.MetacriticMax is not null)
                conditions.Add($"aggregated_rating <= {filters.MetacriticMax.Value}");
        }

        var term = search?.Trim();
        if (string.IsNullOrEmpty(term))
            conditions.Add("total_rating_count > 0");
        else
            conditions.Add($"name ~ *\"{Escape(term)}\"*");

        return $"where {string.Join(" & ", conditions)};";
    }

    private static string BuildOrdering(string? search, GameSearchFilters? filters)
    {
        var ordering = filters?.Ordering;
        var clause = ordering switch
        {
            "-rating" => "total_rating desc",
            "-metacritic" => "aggregated_rating desc",
            "-released" => "first_release_date desc",
            "released" => "first_release_date asc",
            "-added" => "total_rating_count desc",
            "name" => "name asc",
            _ when string.IsNullOrWhiteSpace(search) => "total_rating_count desc",
            _ => "name asc"
        };

        return $" sort {clause};";
    }

    private static string QuotedValues(IEnumerable<string> values) =>
        string.Join(",", values.Select(value => $"\"{Escape(value)}\""));

    /// <summary>
    /// Gets one game by id, or null when IGDB does not know it.
    /// </summary>
    public async Task<GameDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var games = await PostAsync<List<IgdbGame>>(
            "games", $"fields {IgdbGameFields.Detail}; where id = {id}; limit 1;", cancellationToken);

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
    /// Gets the genre and platform options from IGDB, plus the fixed sort orders.
    /// </summary>
    public Task<GameFilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync("igdb:filter-options", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);

            var genresTask = PostAsync<List<IgdbFilterItem>>(
                "genres", "fields id,name,slug; sort name asc; limit 500;", cancellationToken);
            var platformsTask = PostAsync<List<IgdbFilterItem>>(
                "platforms", $"fields id,name; where id = ({IgdbGameFields.PlatformIds}); sort name asc; limit 500;",
                cancellationToken);

            await Task.WhenAll(genresTask, platformsTask);

            var genres = (genresTask.Result ?? [])
                .Where(g => !string.IsNullOrWhiteSpace(g.Slug))
                .Select(g => new FilterOption(g.Slug!, g.Name))
                .ToList();

            var platforms = (platformsTask.Result ?? [])
                .Select(p => new FilterOption(p.Id.ToString(CultureInfo.InvariantCulture), p.Name))
                .ToList();

            return new GameFilterOptions(genres, platforms, GameOrderings.All);
        })!;

    public async Task<IReadOnlyList<CompanyOption>> SearchCompaniesAsync(
        CompanyKind kind, string search, CancellationToken cancellationToken = default)
    {
        var role = kind switch
        {
            CompanyKind.Developer => "developed",
            CompanyKind.Publisher => "published",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        // The IGDB companies endpoint ignores "search", so match the name with a case-insensitive wildcard.
        // IGDB cannot sort by array size, so fetch extra matches and keep the 10 with the most games.
        var companies = await PostAsync<List<IgdbCompany>>(
            "companies", $"fields name,slug,{role}; where name ~ *\"{Escape(search)}\"* & {role} != null; limit 50;",
            cancellationToken) ?? throw new GamesUnavailableException("The game data service returned an empty company response.");

        return companies
            .Select(company => new CompanyOption(
                company.Slug, company.Name,
                (kind == CompanyKind.Developer ? company.Developed : company.Published)?.Count ?? 0))
            .OrderByDescending(c => c.GamesCount).ThenBy(c => c.Name)
            .Take(10)
            .ToList();
    }

    public async Task<IReadOnlyList<GameDetails>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        var candidates = await PostAsync<List<IgdbGame>>(
            "games",
            $"fields {IgdbGameFields.Detail},screenshots.image_id; where total_rating_count > 0; sort total_rating_count desc; limit 50;",
            cancellationToken);

        return FeaturedGames.Select(candidates ?? [], game => game.Id)
            .Select(ToFeatured)
            .ToList();
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
    /// Builds the cover URL from IGDB's image id. <paramref name="size"/> is an IGDB size name such as t_720p or t_1080p.
    /// </summary>
    private string? ToImageUrl(IgdbCover? cover, string size = "t_720p") =>
        string.IsNullOrWhiteSpace(cover?.ImageId)
            ? null
            : $"{_options.ImageBaseUrl.TrimEnd('/')}/{size}/{cover.ImageId}.jpg";

    /// <summary>
    /// Featured games fill the home page hero, so they get a wide 1080p screenshot instead of the portrait cover.
    /// Falls back to the cover when IGDB has no screenshot.
    /// </summary>
    private GameDetails ToFeatured(IgdbGame g) =>
        ToDetails(g) with
        {
            BackgroundImage = ToImageUrl(g.Screenshots?.FirstOrDefault(), "t_1080p") ?? ToImageUrl(g.Cover, "t_1080p")
        };

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
