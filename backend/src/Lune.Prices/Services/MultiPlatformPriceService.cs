using Lune.Prices.Contracts;
using Lune.Shared.Games;
using Microsoft.Extensions.Caching.Memory;

namespace Lune.Prices.Services;

public sealed class MultiPlatformPriceService(
    IEnumerable<IPlatformPriceService> platformServices,
    IGameCatalog catalog,
    IMemoryCache cache)
{
    public async Task<GamePriceComparison?> GetPriceComparisonAsync(
        int gameId, CancellationToken cancellationToken = default)
    {
        int? steamAppId;
        try
        {
            steamAppId = await cache.GetOrCreateAsync($"steam-app-id:{gameId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                var externalIds = await catalog.FindExternalIdsAsync(gameId, cancellationToken);
                var steam = externalIds.FirstOrDefault(e => e.Source.Equals("Steam", StringComparison.OrdinalIgnoreCase));
                return int.TryParse(steam?.Uid, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var appId) ? appId : (int?)null;
            });
        }
        catch (GameCatalogUnavailableException ex)
        {
            throw new PricesUnavailableException(ex.Message, ex);
        }

        if (steamAppId is null)
            return null;

        var platformPrices = new List<PlatformPrice>();
        var errors = new List<string>();

        var services = platformServices.ToList();
        if (services.Count == 0)
            throw new PricesUnavailableException("No platform price services are registered.");

        foreach (var service in services)
        {
            try
            {
                var price = await service.GetPriceAsync(steamAppId.Value, cancellationToken);
                if (price is not null)
                    platformPrices.Add(price);
            }
            catch (Exception ex)
            {
                errors.Add($"{service.PlatformName}: {ex.Message}");
            }
        }

        if (platformPrices.Count == 0)
        {
            var errorDetail = errors.Count > 0
                ? $"Platform errors: {string.Join("; ", errors)}"
                : "No platform returned data.";
            throw new PricesUnavailableException(errorDetail);
        }

        return new GamePriceComparison(gameId, steamAppId.Value, platformPrices);
    }
}

