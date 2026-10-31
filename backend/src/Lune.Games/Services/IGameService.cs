using Lune.Games.Contracts;
using Lune.Shared.Paging;

namespace Lune.Games.Services;

public interface IGamesService
{
    Task<PagedResult<GameSummary>> SearchAsync(
        string? search, int page, int pageSize, GameSearchFilters? filters = null,
        CancellationToken cancellationToken = default);

    // Task<GameFilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken = default);

    // Task<IReadOnlyList<CompanyOption>> SearchCompaniesAsync(
    //     CompanyKind kind, string search, CancellationToken cancellationToken = default);

    // Task<GameDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>Raised when the upstream game API is misconfigured or unreachable.</summary>
public sealed class GamesUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);