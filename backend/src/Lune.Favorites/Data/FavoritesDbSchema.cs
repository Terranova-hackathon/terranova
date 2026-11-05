namespace Lune.Favorites.Data;

/// <summary>One user's bookmark of one game. The pair (UserId, RawgId) is the key, so duplicates are impossible.</summary>
public class Favorite
{
    /// <summary>The Lune user id. A plain column, not a foreign key, so this module owns its data alone.</summary>
    public string UserId { get; set; } = string.Empty;

    // this RawgId is not being used until we decide we want it back in the table in SQL server is deleted but we can add it back later
    // public int RawgId { get; set; }

    public int IgdbId {get; set; }

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
