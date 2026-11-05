using System.Text.Json.Serialization;

namespace Lune.Games.Igdb;

/// <summary>
/// Represents the response returned by the Igdb API.
/// </summary>
internal sealed class IgdbResponse
{
    [JsonPropertyName("results")] public List<IgdbGame>? Results { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}

/// <summary>
/// Represents one game returned by the Igdb API.
/// </summary>
internal sealed class IgdbGame
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("released")] public string? Released { get; set; }
    [JsonPropertyName("background_image")] public string? BackgroundImage { get; set; }
    [JsonPropertyName("rating")] public double Rating { get; set; }
    [JsonPropertyName("metacritic")] public int? Metacritic { get; set; }
    [JsonPropertyName("description_raw")] public string? DescriptionRaw { get; set; }
    [JsonPropertyName("website")] public string? Website { get; set; }
    [JsonPropertyName("genres")] public List<IgdbNamed>? Genres { get; set; }
    [JsonPropertyName("developers")] public List<IgdbNamed>? Developers { get; set; }
    [JsonPropertyName("publishers")] public List<IgdbNamed>? Publishers { get; set; }
    [JsonPropertyName("platforms")] public List<IgdbPlatformEntry>? Platforms { get; set; }
}

/// <summary>
/// Represents a named entity returned by the Igdb API.
/// </summary>
internal sealed class IgdbNamed
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a platform entry returned by the Igdb API.
/// </summary>
internal sealed class IgdbPlatformEntry
{
    [JsonPropertyName("platform")] public IgdbNamed? Platform { get; set; }
}