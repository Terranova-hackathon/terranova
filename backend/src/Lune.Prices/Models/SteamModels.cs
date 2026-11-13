using System.Text.Json.Serialization;

namespace Lune.Prices.Models;

internal sealed class SteamPriceData
{
    public decimal? CurrentPrice { get; set; }
    public decimal? HistoricalLow { get; set; }
    public string? Currency { get; set; }
}

internal sealed class SteamAppResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public SteamAppData? Data { get; set; }
}

internal sealed class SteamAppData
{
    [JsonPropertyName("price_overview")]
    public SteamPriceOverview? PriceOverview { get; set; }
}

internal sealed class SteamPriceOverview
{
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("final")]
    public int? FinalPrice { get; set; }

    [JsonPropertyName("initial")]
    public int? InitialPrice { get; set; }
}
