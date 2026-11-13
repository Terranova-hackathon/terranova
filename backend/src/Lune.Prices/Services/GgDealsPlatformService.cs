using System.Globalization;
using System.Net.Http.Json;
using Lune.Prices.Models;
using Lune.Prices.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Lune.Prices.Services;

internal sealed class GgDealsPlatformService(
    HttpClient http,
    IOptions<GgDealsOptions> options,
    IMemoryCache cache) : IPlatformPriceService
{
    private readonly GgDealsOptions _options = options.Value;

    public string PlatformName => "GG.deals";

    public async Task<PlatformPrice?> GetPriceAsync(int steamAppId, CancellationToken cancellationToken = default)
    {
        var game = await cache.GetOrCreateAsync($"gg-deals:{_options.Region}:{steamAppId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes);
            return await FetchAsync(steamAppId, cancellationToken);
        });

        if (game?.Prices is not { } prices)
            return null;

        return new PlatformPrice(
            Platform: "GG.deals",
            CurrentPrice: prices.CurrentKeyshops ?? prices.CurrentRetail,
            HistoricalLow: (prices.HistoricalKeyshops.HasValue && prices.HistoricalRetail.HasValue)
                ? Math.Min(prices.HistoricalKeyshops.Value, prices.HistoricalRetail.Value)
                : prices.HistoricalKeyshops ?? prices.HistoricalRetail,
            AveragePrice: null,
            Currency: prices.Currency,
            Url: game.Url);
    }

    private async Task<GgDealsGame?> FetchAsync(int steamAppId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new PricesUnavailableException("The GG.deals API key is not configured (GgDeals__ApiKey).");

        var url = $"prices/by-steam-app-id/?ids={steamAppId}&region={Uri.EscapeDataString(_options.Region)}&key={Uri.EscapeDataString(_options.ApiKey)}";

        try
        {
            using var response = await http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new PricesUnavailableException(
                    $"The price service rejected the request ({(int)response.StatusCode})");

            var body = await response.Content.ReadFromJsonAsync<GgDealsResponse>(cancellationToken);
            if (body is not { Success: true })
                throw new PricesUnavailableException("The price service returned an unsuccessful response.");

            return body.Data?.GetValueOrDefault(steamAppId.ToString(CultureInfo.InvariantCulture));
        }
        catch (HttpRequestException ex)
        {
            throw new PricesUnavailableException("The price service could not be reached.", ex);
        }
    }
}
