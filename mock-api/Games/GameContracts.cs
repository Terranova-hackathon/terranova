namespace MockApi.Games;

public record GameSummary(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    double Rating,
    int? Metacritic,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms);

public record GameDetails(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    double Rating,
    int? Metacritic,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms,
    string? Description,
    string? Website,
    IReadOnlyList<string> Developers,
    IReadOnlyList<string> Publishers);

public record FilterOption(string Value, string Name);

public record CompanyOption(string Slug, string Name, int GamesCount);

public record GameFilters(
    IReadOnlyList<FilterOption> Genres,
    IReadOnlyList<FilterOption> Platforms,
    IReadOnlyList<FilterOption> Orderings);

// Shape of an entry in Data/games.json. PlatformIds and Added drive filtering and sorting only.
public class GameRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Released { get; set; }
    public string? BackgroundImage { get; set; }
    public double Rating { get; set; }
    public int? Metacritic { get; set; }
    public List<string> Genres { get; set; } = [];
    public List<string> Platforms { get; set; } = [];
    public List<int> PlatformIds { get; set; } = [];
    public string? Description { get; set; }
    public string? Website { get; set; }
    public List<string> Developers { get; set; } = [];
    public List<string> Publishers { get; set; } = [];
    public int Added { get; set; }

    public GameSummary ToSummary() =>
        new(Id, Name, Released, BackgroundImage, Rating, Metacritic, Genres, Platforms);

    public GameDetails ToDetails() =>
        new(Id, Name, Released, BackgroundImage, Rating, Metacritic, Genres, Platforms,
            Description, Website, Developers, Publishers);
}
