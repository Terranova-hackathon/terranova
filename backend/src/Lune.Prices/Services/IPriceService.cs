using Lune.Prices.Contracts;

namespace Lune.Prices.Services;

public interface IPriceService
{
    /// <summary>Returns null when the game is unknown, has no Steam release, or GG.deals has no data for it.</summary>
    /// <exception cref="PricesUnavailableException">The price provider is misconfigured or could not be reached.</exception>
    Task<GamePrice?> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
}

/// <summary>Raised when the price provider is misconfigured or unreachable.</summary>
public sealed class PricesUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
