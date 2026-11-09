namespace Lune.Games.Services;

internal static class FeaturedGames
{
    public static IReadOnlyList<T> Select<T>(IEnumerable<T> candidates, Func<T, int> getId)
    {
        var games = candidates.DistinctBy(getId).ToArray();
        Random.Shared.Shuffle(games);
        return games.Take(10).ToArray();
    }
}
