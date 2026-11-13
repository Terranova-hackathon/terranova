namespace Lune.Prices.Services;

public interface IPlatformPriceService
{
    /// <summary>The name of this price platform (e.g., "Steam", "Epic", "GOG").</summary>
    string PlatformName { get; }

    /// <summary>Returns platform-specific price data. Returns null if the game is not available on this platform.</summary>
    /// <exception cref="PricesUnavailableException">The price provider is misconfigured or could not be reached.</exception>
    Task<PlatformPrice?> GetPriceAsync(int steamAppId, CancellationToken cancellationToken = default);
}

/// <summary>Price data from a single platform.</summary>
public sealed record PlatformPrice(
    string Platform,
    decimal? CurrentPrice,
    decimal? HistoricalLow,
    decimal? AveragePrice,
    string? Currency,
    string? Url);
