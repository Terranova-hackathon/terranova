namespace Lune.Games.Contracts;

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
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms);

/// <summary>
/// Represents detailed information about a game.
/// </summary>
public sealed record GameDetails(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    double Rating,
    int? Metacritic,
    string? Description,
    string? Website,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms,
    IReadOnlyList<string> Developers,
    IReadOnlyList<string> Publishers);

/// <summary>
/// Already-validated search filters. Lists are OR-ed together within one filter (any selected genre matches);
/// different filters are AND-ed (genre AND platform AND ...).
/// </summary>
public sealed record GameSearchFilters(
    IReadOnlyList<string> Genres,
    IReadOnlyList<int> Platforms,
    IReadOnlyList<string> Developers,
    IReadOnlyList<string> Publishers,
    string? Ordering,
    int? MetacriticMin,
    int? MetacriticMax);
