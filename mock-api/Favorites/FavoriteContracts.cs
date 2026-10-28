namespace MockApi.Favorites;

public record FavoriteDto(int Id, string Name, string? ImageUrl, DateTime AddedAtUtc);

public class FavoriteRow
{
    public required string UserId { get; init; }
    public required int RawgId { get; init; }
    public required string Name { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public FavoriteDto ToDto() => new(RawgId, Name, ImageUrl, CreatedAtUtc);
}
