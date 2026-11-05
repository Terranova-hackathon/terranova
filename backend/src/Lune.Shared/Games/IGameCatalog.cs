namespace Lune.Shared.Games;

/// <summary>The part of a game other modules may copy and store.</summary>
public sealed record GameSnapshot(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    double Rating,
    int? Metacritic,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Platforms);

/// <summary>Lets modules look up a game without referencing the Games module.</summary>
public interface IGameCatalog
{
    /// <summary>Returns null if the game does not exist.</summary>
    /// <exception cref="GameCatalogUnavailableException">The game data provider is misconfigured or could not be reached.</exception>
    Task<GameSnapshot?> FindAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>Raised when the game data provider is misconfigured or unreachable.</summary>
public class GameCatalogUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
