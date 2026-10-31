using Lune.Games.Contracts;
using Lune.Shared.Paging;

namespace Lune.Games.Services;

public interface IGamesService
{
    Task<PagedResult<GameSummary>> SearchAsync(
        string? search, int page, int pageSize, GameSearchFilters? filters = null,
        CancellationToken cancellationToken = default);

    //TODO: implent filter search for games based on GameSearchFilters
}

/// <summary>Raised when the upstream game API is misconfigured or unreachable.</summary>
public sealed class GamesUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
    