using System.Globalization;
using System.Text.RegularExpressions;
using Lune.Games.Contracts;

namespace Lune.Games.EndPoints;

internal static partial class GameSearchFilterParser
{
    [GeneratedRegex("^[a-z0-9][a-z0-9_-]{0,79}$")]
    private static partial Regex SlugPattern();

    public static GameSearchFilters? Parse(
        string? genres,
        string? platforms,
        string? developers,
        string? publishers,
        string? ordering,
        string? metacriticMin,
        string? metacriticMax,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();

        var genreValues = ParseSlugs(genres, "genres", errors);
        var platformValues = ParsePlatforms(platforms, errors);
        var developerValues = ParseSlugs(developers, "developers", errors);
        var publisherValues = ParseSlugs(publishers, "publishers", errors);

        ordering = ordering?.Trim();
        if (!string.IsNullOrEmpty(ordering) && !GameOrderings.IsValid(ordering))
            errors["ordering"] = ["ordering is not one of the supported sort orders."];

        var min = ParseMetacritic(metacriticMin, "metacriticMin", errors);
        var max = ParseMetacritic(metacriticMax, "metacriticMax", errors);
        if (min is not null && max is not null && min > max)
            errors["metacriticMax"] = ["metacriticMin cannot be greater than metacriticMax."];

        return errors.Count == 0
            ? new GameSearchFilters(genreValues, platformValues, developerValues, publisherValues, ordering, min, max)
            : null;
    }

    private static List<string> ParseSlugs(string? raw, string field, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        var values = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.ToLowerInvariant())
            .Distinct()
            .ToList();

        if (values.Any(value => !SlugPattern().IsMatch(value)))
            errors[field] = [$"{field} must be a comma-separated list of slugs."];
        else if (values.Count > 10)
            errors[field] = [$"{field} accepts at most 10 values."];

        return values;
    }

    private static List<int> ParsePlatforms(string? raw, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var ids = new List<int>();
        foreach (var part in parts)
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
            {
                errors["platforms"] = ["platforms must be a comma-separated list of platform ids."];
                return [];
            }

            if (!ids.Contains(id))
                ids.Add(id);
        }

        if (ids.Count > 10)
            errors["platforms"] = ["platforms accepts at most 10 values."];

        return ids;
    }

    private static int? ParseMetacritic(string? raw, string field, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value) &&
            value is >= 0 and <= 100)
            return value;

        errors[field] = [$"{field} must be between 0 and 100."];
        return null;
    }
}
