namespace Lune.Favorites.Data;

/// <summary>One user's bookmark of one game. The pair (UserId, RawgId) is the key, so duplicates are impossible.</summary>
public class Favorite
{
    /// <summary>The Lune user id. A plain column, not a foreign key, so this module owns its data alone.</summary>
    public string UserId { get; set; } = string.Empty;

    public int RawgId { get; set; }

    // Snapshot of the game summary taken when saved, so the favorites list does not need one RAWG call per game.
    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string? Released { get; set; }

    public double Rating { get; set; }

    public int? Metacritic { get; set; }

    public List<string> Genres { get; set; } = [];

    public List<string> Platforms { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>When the snapshot was last copied from RAWG. Lets a later refresh job find stale rows.</summary>
    public DateTime SnapshotUpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
