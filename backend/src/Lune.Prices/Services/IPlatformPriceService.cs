using Lune.Prices.Contracts;

namespace Lune.Prices.Services;

public interface IPlatformPriceService
{
    /// <summary>The name of a price platform (e.g., "Steam", "Epic", "GOG").</summary>
    string PlatformName { get; }

    /// <summary>Returns platform-specific price data. Returns null if the game is not available on this platform.</summary>
    Task<PlatformPrice?> GetPriceAsync(int steamAppId, CancellationToken cancellationToken = default);
}
