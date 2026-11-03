namespace MockApi.Favorites;

public record FavoriteDto(
    int Id,
    string Name,
    string? ImageUrl,
    string? Released,
    double Rating,
    int? Metacritic,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms,
    DateTime AddedAtUtc);

public class FavoriteRow
{
    public required string UserId { get; init; }
    public required int RawgId { get; init; }
    public required string Name { get; init; }
    public string? ImageUrl { get; init; }
    public string? Released { get; init; }
    public double Rating { get; init; }
    public int? Metacritic { get; init; }
    public IReadOnlyList<string> Genres { get; init; } = [];
    public IReadOnlyList<string> Platforms { get; init; } = [];
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public FavoriteDto ToDto() => new(RawgId, Name, ImageUrl, Released, Rating, Metacritic, Genres, Platforms, CreatedAtUtc);
}
