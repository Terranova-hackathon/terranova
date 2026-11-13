using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Lune.Prices.Contracts;
using Lune.Prices.Models;
using Lune.Prices.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Lune.Prices.Services;

internal sealed class GgDealsService(HttpClient http, IOptions<GgDealsOptions> options, IMemoryCache cache) : IPlatformPriceService
{
    private readonly GgDealsOptions _options = options.Value;

    public string PlatformName => "GG.deals";

    public async Task<PlatformPrice?> GetPriceAsync(int steamAppId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"ggdeals-price:{steamAppId}:{_options.Region}";
        var priceData = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes);
            return await FetchAsync(steamAppId, cancellationToken);
        });

        if (priceData is null)
            return null;

        return new PlatformPrice(
            Platform: "GG.deals",
            CurrentPrice: priceData.CurrentPrice,
            HistoricalLow: priceData.HistoricalLow,
            AveragePrice: null,
            Currency: priceData.Currency,
            Url: priceData.Url);
    }

    private async Task<GgDealsPriceData?> FetchAsync(int steamAppId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new PricesUnavailableException("The GG.deals API key is not configured.");

        var url = $"{_options.BaseUrl}prices/by-steam-app-id/?ids={steamAppId}&region={Uri.EscapeDataString(_options.Region)}&key={Uri.EscapeDataString(_options.ApiKey)}";

        try
        {
            using var response = await http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new PricesUnavailableException(
                    $"GG.deals API rejected the request ({(int)response.StatusCode})");

            var body = await response.Content.ReadFromJsonAsync<GgDealsResponse>(cancellationToken);
            if (body is not { Success: true, Data: not null })
                return null;

            var gameData = body.Data.GetValueOrDefault(steamAppId.ToString(CultureInfo.InvariantCulture));
            if (gameData?.Prices is null)
                return null;

            var prices = gameData.Prices;
            var currentPrice = prices.CurrentRetail ?? prices.CurrentKeyshops;
            var historicalLow = (prices.HistoricalKeyshops.HasValue && prices.HistoricalRetail.HasValue)
                ? Math.Min(prices.HistoricalKeyshops.Value, prices.HistoricalRetail.Value)
                : prices.HistoricalKeyshops ?? prices.HistoricalRetail;

            if (currentPrice is null)
                return null;

            return new GgDealsPriceData
            {
                CurrentPrice = currentPrice,
                HistoricalLow = historicalLow,
                Currency = prices.Currency,
                Url = gameData.Url
            };
        }
        catch (HttpRequestException ex)
        {
            throw new PricesUnavailableException("GG.deals API could not be reached.", ex);
        }
        catch (JsonException ex)
        {
            throw new PricesUnavailableException($"GG.deals API response parsing failed: {ex.Message}", ex);
        }
    }
}
