using System.Text.RegularExpressions;

namespace Lune.Shared.Validation;

/// <summary>
/// Provides validation for slugs.
/// </summary>
public static partial class Slug
{
    // 1-80 chars: starts with a lowercase letter or digit, then letters, digits, '_' or '-'.
    [GeneratedRegex("^[a-z0-9][a-z0-9_-]{0,79}$")]
    private static partial Regex Pattern();

    public static bool IsValid(string? value) => value is not null && Pattern().IsMatch(value);
}
