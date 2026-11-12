using System.Text.Json.Serialization;

namespace Lune.Prices.Models;

/// <summary>Response of GG.deals prices/by-steam-app-id. Data is keyed by Steam app id; games GG.deals does not know are null.</summary>
internal sealed class GgDealsResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("data")] public Dictionary<string, GgDealsGame?>? Data { get; set; }
}

internal sealed class GgDealsGame
{
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("prices")] public GgDealsPrices? Prices { get; set; }
}

/// <summary>GG.deals sends prices as decimal strings, so reading numbers from strings must be allowed.</summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
internal sealed class GgDealsPrices
{
    [JsonPropertyName("currentRetail")] public decimal? CurrentRetail { get; set; }
    [JsonPropertyName("currentKeyshops")] public decimal? CurrentKeyshops { get; set; }
    [JsonPropertyName("historicalRetail")] public decimal? HistoricalRetail { get; set; }
    [JsonPropertyName("historicalKeyshops")] public decimal? HistoricalKeyshops { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
}
