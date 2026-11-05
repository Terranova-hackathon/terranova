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
    /// <summary>Unix timestamp in seconds.</summary>
    [JsonPropertyName("first_release_date")] public long? FirstReleaseDate { get; set; }
    [JsonPropertyName("cover")] public IgdbCover? Cover { get; set; }
    /// <summary>User and critic rating combined, 0-100.</summary>
    [JsonPropertyName("total_rating")] public double? TotalRating { get; set; }
    /// <summary>Critic rating, 0-100.</summary>
    [JsonPropertyName("aggregated_rating")] public double? AggregatedRating { get; set; }
    [JsonPropertyName("summary")] public string? Summary { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("genres")] public List<IgdbNamed>? Genres { get; set; }
    [JsonPropertyName("platforms")] public List<IgdbNamed>? Platforms { get; set; }
    [JsonPropertyName("involved_companies")] public List<IgdbInvolvedCompany>? InvolvedCompanies { get; set; }
}

/// <summary>
/// Represents a named entity (genre, platform, company) returned by the IGDB API.
/// </summary>
internal sealed class IgdbNamed
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
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