using System.Text.RegularExpressions;
using MockApi.Shared;

namespace MockApi.Games;

public static partial class GamesEndpoints
{
    [GeneratedRegex("^[a-z0-9][a-z0-9_-]{0,79}$")]
    private static partial Regex SlugPattern();

    public static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/games")
            .RequireSession()
            .AddEndpointFilter(Simulate503);

        group.MapGet("", (HttpRequest req, GameCatalog catalog) => Search(req, catalog));

        group.MapGet("/filters", () =>
            Results.Ok(new GameFilters(GameCatalog.Genres, GameCatalog.Platforms, GameCatalog.Orderings)));

        group.MapGet("/companies", (string? kind, string? search, GameCatalog catalog) =>
        {
            var errors = new Dictionary<string, string[]>();
            var normalizedKind = kind?.Trim().ToLowerInvariant();
            if (normalizedKind is not ("developer" or "publisher"))
                errors["kind"] = ["kind must be 'developer' or 'publisher'."];
            var term = search?.Trim() ?? "";
            if (term.Length is < 2 or > 60)
                errors["search"] = ["search must be between 2 and 60 characters."];
            if (errors.Count > 0) return Problems.Validation(errors);

            return Results.Ok(catalog.SearchCompanies(normalizedKind!, term));
        });

        group.MapGet("/featured", (GameCatalog catalog) =>
            Results.Ok((catalog.Games.FirstOrDefault(g => g.Name.StartsWith("The Witcher 3")) ?? catalog.Games[0]).ToDetails()));

        group.MapGet("/{id:int}", (int id, GameCatalog catalog) =>
            catalog.Find(id) is { } game ? Results.Ok(game.ToDetails()) : Results.NotFound());
    }

    private static IResult Search(HttpRequest req, GameCatalog catalog)
    {
        var q = req.Query;
        var errors = new Dictionary<string, string[]>();

        var search = q["search"].ToString().Trim();
        var page = int.TryParse(q["page"], out var p) ? Math.Max(p, 1) : 1;
        var pageSize = int.TryParse(q["pageSize"], out var ps) ? Math.Clamp(ps, 1, 40) : 20;

        var genres = ParseSlugs(q["genres"].ToString(), "genres", errors);
        var developers = ParseSlugs(q["developers"].ToString(), "developers", errors);
        var publishers = ParseSlugs(q["publishers"].ToString(), "publishers", errors);
        var platforms = ParsePlatforms(q["platforms"].ToString(), errors);

        var ordering = q["ordering"].ToString();
        if (ordering.Length > 0 && !GameCatalog.Orderings.Any(o => o.Value == ordering))
            errors["ordering"] = ["ordering is not one of the supported sort orders."];

        var min = ParseMetacritic(q["metacriticMin"].ToString(), "metacriticMin", errors);
        var max = ParseMetacritic(q["metacriticMax"].ToString(), "metacriticMax", errors);
        if (min is not null && max is not null && min > max)
            errors["metacriticMax"] = ["metacriticMin cannot be greater than metacriticMax."];

        if (errors.Count > 0) return Problems.Validation(errors);

        IEnumerable<GameRecord> games = catalog.Games;
        if (search.Length > 0)
            games = games.Where(g => g.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (genres.Count > 0)
            games = games.Where(g => g.Genres.Any(n => genres.Any(s => GameCatalog.GenreName(s) == n)));
        if (platforms.Count > 0)
            games = games.Where(g => g.PlatformIds.Any(platforms.Contains));
        if (developers.Count > 0)
            games = games.Where(g => g.Developers.Any(d => developers.Contains(GameCatalog.Slugify(d))));
        if (publishers.Count > 0)
            games = games.Where(g => g.Publishers.Any(d => publishers.Contains(GameCatalog.Slugify(d))));
        if (min is not null)
            games = games.Where(g => g.Metacritic >= min);
        if (max is not null)
            games = games.Where(g => g.Metacritic <= max);

        games = ordering switch
        {
            "-rating" => games.OrderByDescending(g => g.Rating),
            "-metacritic" => games.OrderByDescending(g => g.Metacritic ?? -1),
            "-released" => games.OrderByDescending(g => g.Released),
            "released" => games.OrderBy(g => g.Released),
            "-added" => games.OrderByDescending(g => g.Added),
            "name" => games.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase),
            _ => games
        };

        var all = games.ToList();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(g => g.ToSummary()).ToList();
        var totalPages = (int)Math.Ceiling(all.Count / (double)pageSize);
        return Results.Ok(new PagedResult<GameSummary>(items, page, pageSize, all.Count, totalPages));
    }

    private static List<string> ParseSlugs(string raw, string field, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        var values = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v.ToLowerInvariant()).Distinct().ToList();
        if (values.Any(v => !SlugPattern().IsMatch(v)))
            errors[field] = [$"{field} must be a comma-separated list of slugs."];
        else if (values.Count > 10)
            errors[field] = [$"{field} accepts at most 10 values."];
        return values;
    }

    private static List<int> ParsePlatforms(string raw, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var ids = new List<int>();
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var id) || id <= 0)
            {
                errors["platforms"] = ["platforms must be a comma-separated list of platform ids."];
                return [];
            }
            ids.Add(id);
        }
        if (ids.Count > 10) errors["platforms"] = ["platforms accepts at most 10 values."];
        return ids;
    }

    private static int? ParseMetacritic(string raw, string field, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (int.TryParse(raw, out var value) && value is >= 0 and <= 100) return value;
        errors[field] = [$"{field} must be between 0 and 100."];
        return null;
    }

    private static async ValueTask<object?> Simulate503(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) =>
        Problems.Wants503(ctx.HttpContext.Request) ? Problems.ProviderUnavailable() : await next(ctx);
}
