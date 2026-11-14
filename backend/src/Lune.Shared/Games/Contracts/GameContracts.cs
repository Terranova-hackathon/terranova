using Lune.Shared.Paging;

namespace Lune.Shared.Games.Contracts;


/// <summary>
/// Contains contract definitions for game-related data.
/// </summary>
public sealed record GameSummary(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    double Rating,
    int? Metacritic,
    IReadOnlyList<FilterOption> Genres,
    IReadOnlyList<FilterOption> Platforms);

/// <summary>
/// Represents detailed information about a game.
/// </summary>
public sealed record GameDetails(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    string? Cover,
    double Rating,
    int? Metacritic,
    string? Description,
    string? StoryLine,
    string? Website,
    IReadOnlyList<ExternalGameLink> ExternalGames,
    IReadOnlyList<FilterOption> Genres,
    IReadOnlyList<FilterOption> Platforms,
    IReadOnlyList<FilterOption> Developers,
    IReadOnlyList<FilterOption> Publishers,
    IReadOnlyList<string> Screenshots,
    IReadOnlyList<string> Artworks,
    IReadOnlyList<string> Videos,
    PagedResult<GameSummary> RelatedGames);

/// <summary>
/// The game's id on another store or service.
/// </summary>
public sealed record ExternalGameLink(
    string Source,
    string Uid,
    string? Url);

/// <summary>
/// Represents a filterable option (genre, platform, etc) with value and display name.
/// </summary>
public sealed record FilterOption(
    string Value,
    string Name);

/// <summary>
/// Filter options available for game search.
/// </summary>
public sealed record GameFilterOptions(
    IReadOnlyList<FilterOption> Genres,
    IReadOnlyList<FilterOption> Platforms,
    IReadOnlyList<FilterOption> Orderings);

/// <summary>
/// A developer or publisher company option for search.
/// </summary>
public sealed record CompanyOption(
    string Slug,
    string Name,
    int GamesCount);

/// <summary>
/// For internal use only.
/// </summary>
public enum CompanyKind
{
    Developer,
    Publisher,
}

/// <summary>
/// The only sort orders we accept. A leading "-" means descending.
/// </summary>
public static class GameOrderings
{
    public static readonly IReadOnlyList<FilterOption> All =
    [
        new("-rating", "Rating (high to low)"),
        new("-metacritic", "Metacritic (high to low)"),
        new("-released", "Release date (newest)"),
        new("released", "Release date (oldest)"),
        new("-added", "Most popular"),
        new("name", "Name (A-Z)"),
    ];

    public static bool IsValid(string value) => All.Any(o => o.Value == value);
}

/// <summary>
/// Already-validated search filters. Lists are OR-ed together within one filter (any selected genre matches);
/// different filters are AND-ed (genre AND platform AND ...). frontend -> backend.
/// </summary>
public sealed record GameSearchFilters(
    IReadOnlyList<string> Genres,
    IReadOnlyList<int> Platforms,
    IReadOnlyList<string> Developers,
    IReadOnlyList<string> Publishers,
    string? Ordering,
    int? MetacriticMin,
    int? MetacriticMax);
