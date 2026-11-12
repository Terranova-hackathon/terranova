using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Lune.Prices.Contracts;
using Lune.Prices.Models;
using Lune.Prices.Options;
using Lune.Shared.Games;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Lune.Prices.Services;

internal sealed class GgDealsPriceService(
    HttpClient http, IOptions<GgDealsOptions> options, IGameCatalog catalog, IMemoryCache cache) : IPriceService
{
    private readonly GgDealsOptions _options = options.Value;

    public async Task<GamePrice?> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        int? steamAppId;
        try
        {
            steamAppId = await cache.GetOrCreateAsync($"steam-app-id:{gameId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                var externalIds = await catalog.FindExternalIdsAsync(gameId, cancellationToken);
                var steam = externalIds.FirstOrDefault(e => e.Source.Equals("Steam", StringComparison.OrdinalIgnoreCase));
                return int.TryParse(steam?.Uid, NumberStyles.None, CultureInfo.InvariantCulture, out var appId) ? appId : (int?)null;
            });
        }
        catch (GameCatalogUnavailableException ex)
        {
            throw new PricesUnavailableException(ex.Message, ex);
        }

        if (steamAppId is null)
            return null;

        var game = await cache.GetOrCreateAsync($"gg-deals:{_options.Region}:{steamAppId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes);
            return await FetchAsync(steamAppId.Value, cancellationToken);
        });

        if (game?.Prices is not { } prices)
            return null;

        return new GamePrice(
            gameId,
            steamAppId.Value,
            _options.Region,
            prices.Currency,
            prices.CurrentRetail,
            prices.CurrentKeyshops,
            prices.HistoricalRetail,
            prices.HistoricalKeyshops,
            DealEvaluator.Evaluate(prices.CurrentRetail, prices.HistoricalRetail),
            game.Url);
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
                    $"The price service rejected the request ({(int)response.StatusCode}): {await ReadErrorMessageAsync(response, cancellationToken)}");

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

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            if (json.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("message", out var message))
                return message.GetString() ?? response.ReasonPhrase ?? "unknown error";
        }
        catch (JsonException)
        {
        }

        return response.ReasonPhrase ?? "unknown error";
    }
}
