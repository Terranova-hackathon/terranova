using System.Text.Json.Serialization;

namespace Lune.Games.Rawg;

/// <summary>
/// Represents the response returned by the RAWG API.
/// </summary>
internal sealed class RawgResponse
{
    [JsonPropertyName("results")] public List<RawgGame>? Results { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}

/// <summary>
/// Represents one game returned by the RAWG API.
/// </summary>
internal sealed class RawgGame
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("released")] public string? Released { get; set; }
    [JsonPropertyName("background_image")] public string? BackgroundImage { get; set; }
    [JsonPropertyName("rating")] public double Rating { get; set; }
    [JsonPropertyName("metacritic")] public int? Metacritic { get; set; }
    [JsonPropertyName("description_raw")] public string? DescriptionRaw { get; set; }
    [JsonPropertyName("website")] public string? Website { get; set; }
    [JsonPropertyName("genres")] public List<RawgNamed>? Genres { get; set; }
    [JsonPropertyName("developers")] public List<RawgNamed>? Developers { get; set; }
    [JsonPropertyName("publishers")] public List<RawgNamed>? Publishers { get; set; }
    [JsonPropertyName("platforms")] public List<RawgPlatformEntry>? Platforms { get; set; }
}

/// <summary>
/// Represents a named entity returned by the RAWG API.
/// </summary>
internal sealed class RawgNamed
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a platform entry returned by the RAWG API.
/// </summary>

internal sealed class RawgPlatformEntry
{
    [JsonPropertyName("platform")] public RawgNamed? Platform { get; set; }
}


// TODO: Implement Filter options for the RAWG API.