using System.Text.Json.Serialization;

namespace Lune.Games.Igdb;

/// <summary>
/// Represents the response of IGDB's /games/count endpoint.
/// </summary>
internal sealed class IgdbCount
{
    [JsonPropertyName("count")] public int Count { get; set; }
}

/// <summary>
/// Represents one game returned by the IGDB /games endpoint. The /games endpoint returns a plain JSON array of these.
/// </summary>
internal sealed class IgdbGame
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("first_release_date")] public long? FirstReleaseDate { get; set; }
    [JsonPropertyName("cover")] public IgdbCover? Cover { get; set; }
    [JsonPropertyName("screenshots")] public List<IgdbCover>? Screenshots { get; set; }
    [JsonPropertyName("artworks")] public List<IgdbCover>? Artworks { get; set; }
    [JsonPropertyName("total_rating")] public double? TotalRating { get; set; }
    [JsonPropertyName("aggregated_rating")] public double? AggregatedRating { get; set; }
    [JsonPropertyName("summary")] public string? Summary { get; set; }
    [JsonPropertyName("storyline")] public string? Storyline { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("websites")] public List<IgdbWebsite>? Websites { get; set; }
    [JsonPropertyName("genres")] public List<IgdbNamed>? Genres { get; set; }
    [JsonPropertyName("platforms")] public List<IgdbNamed>? Platforms { get; set; }
    [JsonPropertyName("involved_companies")] public List<IgdbInvolvedCompany>? InvolvedCompanies { get; set; }
    [JsonPropertyName("videos")] public List<IgdbVideo>? Videos { get; set; }
    [JsonPropertyName("similar_games")] public List<int>? SimilarGames { get; set; }
    [JsonPropertyName("external_games")] public List<IgdbExternalGame>? ExternalGames { get; set; }
}

/// <summary>
/// Maps a game to its id on another store or service (Steam, GOG, Epic, ...). The uid is that service's own id for the game.
/// </summary>
internal sealed class IgdbExternalGame
{
    [JsonPropertyName("uid")] public string? Uid { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("external_game_source")] public IgdbExternalSource? ExternalGameSource { get; set; }
}

/// <summary>
/// The store or service an external game id belongs to.
/// </summary>
internal sealed class IgdbExternalSource
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

/// <summary>
/// Represents a named entity (genre, platform, company) returned by the IGDB API.
/// </summary>
internal sealed class IgdbNamed
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("slug")] public string? Slug { get; set; }
}

/// <summary>
/// A company and its game IDs for the requested role.
/// </summary>
internal sealed class IgdbCompany
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("developed")] public List<int>? Developed { get; set; }
    [JsonPropertyName("published")] public List<int>? Published { get; set; }
}

/// <summary>
/// Represents a game's cover. IGDB only gives an image id, the URL has to be built from it.
/// </summary>
internal sealed class IgdbCover
{
    [JsonPropertyName("image_id")] public string? ImageId { get; set; }
}

/// <summary>
/// Links a game to a company and says whether that company developed and/or published it.
/// </summary>
internal sealed class IgdbInvolvedCompany
{
    [JsonPropertyName("developer")] public bool Developer { get; set; }
    [JsonPropertyName("publisher")] public bool Publisher { get; set; }
    [JsonPropertyName("company")] public IgdbNamed? Company { get; set; }
}

/// <summary>
/// Represents a website URL for a game (official site, Steam, etc).
/// </summary>
internal sealed class IgdbWebsite
{
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("category")] public int Category { get; set; }
}

/// <summary>
/// Represents a video (trailer, gameplay, etc) for a game.
/// </summary>
internal sealed class IgdbVideo
{
    [JsonPropertyName("video_id")] public string? VideoId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}