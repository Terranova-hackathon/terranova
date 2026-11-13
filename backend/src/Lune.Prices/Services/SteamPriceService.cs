using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lune.Prices.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Lune.Prices.Services;

internal sealed class SteamPriceService(HttpClient http, IOptions<SteamOptions> options, IMemoryCache cache) : IPlatformPriceService
{
    private const string BaseUrl = "https://store.steampowered.com/api/appdetails";
    private readonly SteamOptions _options = options.Value;

    public string PlatformName => "Steam";

    public async Task<PlatformPrice?> GetPriceAsync(int steamAppId, CancellationToken cancellationToken = default)
    {
        var priceData = await cache.GetOrCreateAsync($"steam-price:{steamAppId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes);
            return await FetchAsync(steamAppId, cancellationToken);
        });

        if (priceData is null)
            return null;

        return new PlatformPrice(
            Platform: "Steam",
            CurrentPrice: priceData.CurrentPrice,
            HistoricalLow: priceData.HistoricalLow,
            AveragePrice: null,
            Currency: priceData.Currency,
            Url: $"https://store.steampowered.com/app/{steamAppId}");
    }

    private async Task<SteamPriceData?> FetchAsync(int steamAppId, CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}?appids={steamAppId}&cc={_options.CurrencyCode}";

        try
        {
            using var response = await http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new PricesUnavailableException(
                    $"Steam API rejected the request ({(int)response.StatusCode})");

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, SteamAppResponse>>(
                cancellationToken: cancellationToken);

            if (body is null || !body.TryGetValue(steamAppId.ToString(CultureInfo.InvariantCulture), out var appData))
                return null;

            if (!appData.Success)
                return null;

            var priceOverview = appData.Data?.PriceOverview;
            if (priceOverview is null)
                return null;

            var currentPrice = priceOverview.FinalPrice.HasValue ? (decimal?)(priceOverview.FinalPrice.Value / 100m) : null;

            return new SteamPriceData
            {
                CurrentPrice = currentPrice,
                HistoricalLow = null,
                Currency = priceOverview.Currency
            };
        }
        catch (HttpRequestException ex)
        {
            throw new PricesUnavailableException("Steam API could not be reached.", ex);
        }
        catch (JsonException ex)
        {
            throw new PricesUnavailableException($"Steam API response parsing failed: {ex.Message}", ex);
        }
    }

    private sealed class SteamPriceData
    {
        public decimal? CurrentPrice { get; set; }
        public decimal? HistoricalLow { get; set; }
        public string? Currency { get; set; }
    }

    private sealed class SteamAppResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public SteamAppData? Data { get; set; }
    }

    private sealed class SteamAppData
    {
        [JsonPropertyName("price_overview")]
        public SteamPriceOverview? PriceOverview { get; set; }
    }

    private sealed class SteamPriceOverview
    {
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("final")]
        public int? FinalPrice { get; set; }

        [JsonPropertyName("initial")]
        public int? InitialPrice { get; set; }
    }
}
