namespace Lune.Favorites.Contracts;

/// <summary>A favorited game as returned to the client. <see cref="Id"/> is the RAWG game id.</summary>
public sealed record FavoriteResponse(
    int Id,
    string Name,
    string? ImageUrl,
    string? Released,
    double Rating,
    int? Metacritic,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms,
    DateTime AddedAtUtc);
