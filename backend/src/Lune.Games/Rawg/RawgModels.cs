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
    [JsonPropertyName("genres")] public List<RawgOption>? Genres { get; set; }
    [JsonPropertyName("developers")] public List<RawgOption>? Developers { get; set; }
    [JsonPropertyName("publishers")] public List<RawgOption>? Publishers { get; set; }
    [JsonPropertyName("platforms")] public List<RawgPlatformEntry>? Platforms { get; set; }
}

/// <summary>
/// Represents a platform entry returned by the RAWG API.
/// </summary>
internal sealed class RawgPlatformEntry
{
    [JsonPropertyName("platform")] public RawgOption? Platform { get; set; }
}

/// <summary>
/// Represents the list response of RAWG's genres, platforms, developers and publishers endpoints.
/// </summary>
internal sealed class RawgOptionList
{
    [JsonPropertyName("results")] public List<RawgOption> Results { get; set; } = [];
}

/// <summary>
/// Represents a genre, platform, developer or publisher returned by the RAWG API.
/// Fields RAWG omits in a given context keep their defaults.
/// </summary>
internal sealed class RawgOption
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
    [JsonPropertyName("games_count")] public int GamesCount { get; set; }
}
