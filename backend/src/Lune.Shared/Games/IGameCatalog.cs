using Lune.Shared.Games.Contracts;

namespace Lune.Shared.Games;

/// <summary>The part of a game other modules may copy and store.</summary>
public sealed record GameSnapshot(
    int Id,
    string Name,
    string? Released,
    string? BackgroundImage,
    double Rating,
    int? Metacritic,
    IReadOnlyList<FilterOption> Genres,
    IReadOnlyList<FilterOption> Platforms);

/// <summary>Lets modules look up a game without referencing the Games module.</summary>
public interface IGameCatalog
{
    /// <summary>Returns null if the game does not exist.</summary>
    /// <exception cref="GameCatalogUnavailableException">The game data provider is misconfigured or could not be reached.</exception>
    Task<GameSnapshot?> FindAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Returns the game's ids on other stores and services (Steam, GOG, PlayStation Store, ...). Empty if the game is unknown.</summary>
    /// <exception cref="GameCatalogUnavailableException">The game data provider is misconfigured or could not be reached.</exception>
    Task<IReadOnlyList<ExternalGameId>> FindExternalIdsAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>A game's id on another store or service, e.g. Source "Steam" with Uid "367520".</summary>
public sealed record ExternalGameId(string Source, string Uid, string? Url);

/// <summary>Raised when the game data provider is misconfigured or unreachable.</summary>
public class GameCatalogUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
