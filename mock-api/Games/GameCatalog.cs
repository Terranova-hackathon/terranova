using System.Text.Json;
using System.Text.RegularExpressions;

namespace MockApi.Games;

public class GameCatalog
{
    public static readonly IReadOnlyList<FilterOption> Genres =
    [
        new("action", "Action"), new("indie", "Indie"), new("adventure", "Adventure"),
        new("role-playing-games-rpg", "RPG"), new("shooter", "Shooter"), new("strategy", "Strategy"),
        new("puzzle", "Puzzle"), new("platformer", "Platformer"), new("racing", "Racing"),
        new("sports", "Sports"), new("simulation", "Simulation"), new("fighting", "Fighting")
    ];

    public static readonly IReadOnlyList<FilterOption> Platforms =
    [
        new("1", "PC"), new("2", "PlayStation"), new("3", "Xbox"), new("7", "Nintendo")
    ];

    public static readonly IReadOnlyList<FilterOption> Orderings =
    [
        new("-rating", "Rating (high to low)"), new("-metacritic", "Metacritic (high to low)"),
        new("-released", "Release date (newest)"), new("released", "Release date (oldest)"),
        new("-added", "Most popular"), new("name", "Name (A-Z)")
    ];

    public IReadOnlyList<GameRecord> Games { get; }

    public GameCatalog(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "games.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Games = JsonSerializer.Deserialize<List<GameRecord>>(File.ReadAllText(path), options) ?? [];
    }

    public GameRecord? Find(int id) => Games.FirstOrDefault(g => g.Id == id);

    public static string GenreName(string slug) => Genres.FirstOrDefault(g => g.Value == slug)?.Name ?? slug;

    public static string Slugify(string name) =>
        Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    public IReadOnlyList<CompanyOption> SearchCompanies(string kind, string search)
    {
        var names = kind == "developer" ? Games.SelectMany(g => g.Developers) : Games.SelectMany(g => g.Publishers);
        return names
            .GroupBy(n => n)
            .Where(g => g.Key.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(g => new CompanyOption(Slugify(g.Key), g.Key, g.Count()))
            .OrderByDescending(c => c.GamesCount).ThenBy(c => c.Name)
            .Take(10)
            .ToList();
    }
}
