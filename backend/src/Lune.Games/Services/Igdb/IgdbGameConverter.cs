using System.Globalization;
using Lune.Games.Contracts;
using Lune.Games.Igdb;
using Lune.Games.Options;
using Lune.Shared.Paging;

namespace Lune.Games.Services;

/// <summary>
/// Converts IGDB game models to API contracts.
/// </summary>
internal sealed class IgdbGameConverter(IgdbServiceOptions options)
{
    /// <summary>
    /// Escapes text so it can sit inside a quoted IGDB string.
    /// </summary>
    internal static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>
    /// Extracts the names from a list of IGDB named items.
    /// </summary>
    internal static IReadOnlyList<string> Names(List<IgdbNamed>? items) =>
        items?.Select(i => i.Name).ToList() ?? [];

    /// <summary>
    /// Extracts the slug/name pairs from a list of IGDB named items, so the frontend can query by slug.
    /// </summary>
    internal static IReadOnlyList<FilterOption> Options(List<IgdbNamed>? items) =>
        items?.Where(i => !string.IsNullOrWhiteSpace(i.Slug))
            .Select(i => new FilterOption(i.Slug!, i.Name))
            .ToList() ?? [];

    /// <summary>
    /// Extracts the ID/name pairs from a list of IGDB named items (for platforms), so the frontend can query by ID.
    /// </summary>
    internal static IReadOnlyList<FilterOption> IdOptions(List<IgdbNamed>? items) =>
        items?.Where(i => i.Id.HasValue)
            .Select(i => new FilterOption(i.Id!.Value.ToString(CultureInfo.InvariantCulture), i.Name))
            .ToList() ?? [];

    /// <summary>
    /// Extracts the slug/name pairs of the companies that have the given role (developer or publisher).
    /// </summary>
    internal static IReadOnlyList<FilterOption> CompanyOptions(List<IgdbInvolvedCompany>? items, Func<IgdbInvolvedCompany, bool> role) =>
        items?.Where(c => role(c) && c.Company is not null && !string.IsNullOrWhiteSpace(c.Company.Slug))
            .Select(c => new FilterOption(c.Company!.Slug!, c.Company!.Name))
            .Distinct()
            .ToList() ?? [];

    /// <summary>
    /// Converts IGDB's Unix timestamp to the YYYY-MM-DD string the API contract uses.
    /// </summary>
    internal static string? ToReleaseDate(long? unixSeconds) =>
        unixSeconds is null
            ? null
            : DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// Builds the cover URL from IGDB's image id. <paramref name="size"/> is an IGDB size name such as t_720p or t_1080p.
    /// </summary>
    internal string? ToImageUrl(IgdbCover? cover, string size = "t_720p") =>
        string.IsNullOrWhiteSpace(cover?.ImageId)
            ? null
            : $"{options.ImageBaseUrl.TrimEnd('/')}/{size}/{cover.ImageId}.jpg";

    /// <summary>
    /// Builds a list of image URLs from IGDB cover/artwork objects.
    /// </summary>
    internal IReadOnlyList<string> ToImageUrls(List<IgdbCover>? covers, string size = "t_720p") =>
        covers?.Select(c => ToImageUrl(c, size))
            .Where(url => url is not null)
            .Cast<string>()
            .ToList() ?? [];

    /// <summary>
    /// Builds YouTube video URLs from IGDB video IDs.
    /// </summary>
    internal IReadOnlyList<string> ToVideoUrls(List<IgdbVideo>? videos) =>
        videos?.Select(v => $"{options.VideoBaseUrl}{v.VideoId}")
            .Where(url => !string.IsNullOrWhiteSpace(url) && !url.EndsWith("="))
            .ToList() ?? [];

    /// <summary>
    /// Extracts the official website URL from IGDB's websites list (category 1 = official website).
    /// </summary>
    internal static string? ToWebsiteUrl(List<IgdbWebsite>? websites) =>
        websites?.FirstOrDefault(w => w.Category == 1)?.Url;

    private const int SteamExternalGameSource = 1;

    /// <summary>
    /// Finds the Steam app id in IGDB's external games list (source 1 = Steam).
    /// </summary>
    internal static int? ToSteamAppId(List<IgdbExternalGame>? externalGames) =>
        externalGames?
            .Where(e => e.ExternalGameSource?.Id == SteamExternalGameSource)
            .Select(e => int.TryParse(e.Uid, NumberStyles.None, CultureInfo.InvariantCulture, out var appId) ? appId : (int?)null)
            .FirstOrDefault(appId => appId is not null);

    /// <summary>
    /// Lists the game's ids on other stores and services, skipping excluded sources and entries IGDB has no source name or uid for.
    /// </summary>
    internal static IReadOnlyList<ExternalGameLink> ToExternalGames(List<IgdbExternalGame>? externalGames) =>
        externalGames?
            .Where(e => !string.IsNullOrWhiteSpace(e.Uid) && !string.IsNullOrWhiteSpace(e.ExternalGameSource?.Name))
            .Where(e => !IsExcludedExternalSource(e.ExternalGameSource!.Name!))
            .Select(e => new ExternalGameLink(e.ExternalGameSource!.Name!, e.Uid!, e.Url))
            .ToList() ?? [];

    private static bool IsExcludedExternalSource(string name) =>
        name.StartsWith("Amazon", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("YouTube", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Picks a random screenshot for the background image, or falls back to the cover.
    /// </summary>
    internal string? ToBackgroundImage(List<IgdbCover>? screenshots, IgdbCover? cover)
    {
        if (screenshots?.Count > 0)
        {
            var random = new Random();
            var randomScreenshot = screenshots[random.Next(screenshots.Count)];
            var url = ToImageUrl(randomScreenshot, "t_1080p");
            if (url is not null)
                return url;
        }
        return ToImageUrl(cover);
    }

    /// <summary>
    /// Converts IGDB's 0-100 rating to the 0-5 scale the API contract uses.
    /// </summary>
    internal static double ToRating(double? totalRating) =>
        totalRating is null ? 0 : Math.Round(totalRating.Value / 20, 2);

    /// <summary>
    /// IGDB has no Metacritic score, so the critic rating (0-100) stands in for it.
    /// </summary>
    internal static int? ToMetacritic(double? aggregatedRating) =>
        aggregatedRating is null ? null : (int)Math.Round(aggregatedRating.Value);

    /// <summary>
    /// Converts an IGDB game object to a game summary.
    /// </summary>
    internal GameSummary ToSummary(IgdbGame g) => new(
        g.Id,
        g.Name,
        ToReleaseDate(g.FirstReleaseDate),
        ToImageUrl(g.Cover),
        ToRating(g.TotalRating),
        ToMetacritic(g.AggregatedRating),
        Options(g.Genres),
        IdOptions(g.Platforms));

    /// <summary>
    /// Converts an IGDB game object to full game details.
    /// </summary>
    internal GameDetails ToDetails(IgdbGame g, PagedResult<GameSummary> relatedGames) => new(
        Id: g.Id,
        Name: g.Name,
        Released: ToReleaseDate(g.FirstReleaseDate),
        BackgroundImage: ToBackgroundImage(g.Screenshots, g.Cover),
        Cover: ToImageUrl(g.Cover, "t_cover_big"),
        Rating: ToRating(g.TotalRating),
        Metacritic: ToMetacritic(g.AggregatedRating),
        Description: g.Summary,
        StoryLine: g.Storyline,
        Website: ToWebsiteUrl(g.Websites),
        ExternalGames: ToExternalGames(g.ExternalGames),
        Genres: Options(g.Genres),
        Platforms: IdOptions(g.Platforms),
        Developers: CompanyOptions(g.InvolvedCompanies, c => c.Developer),
        Publishers: CompanyOptions(g.InvolvedCompanies, c => c.Publisher),
        Screenshots: ToImageUrls(g.Screenshots, "t_1080p"),
        Artworks: ToImageUrls(g.Artworks, "t_1080p"),
        Videos: ToVideoUrls(g.Videos),
        RelatedGames: relatedGames);

    /// <summary>
    /// Converts an IGDB game object to featured game details with a random screenshot background.
    /// </summary>
    internal GameDetails ToFeatured(IgdbGame g) =>
        ToDetails(g, new PagedResult<GameSummary>([], 1, 5, 0)) with
        {
            BackgroundImage = ToImageUrl(g.Screenshots?.FirstOrDefault(), "t_1080p") ?? ToImageUrl(g.Cover, "t_1080p")
        };
}
