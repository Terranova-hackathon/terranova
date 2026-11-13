using System.Text.Json.Serialization;

namespace Lune.Prices.Contracts;

/// <summary>How the current official retail price compares with the lowest price GG.deals has recorded.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DealVerdict
{
    Unknown,
    HistoricalLow,
    GreatDeal,
    GoodDeal,
    Fair,
    Wait,
}

/// <summary>Current and historical-low prices for one game in one region.</summary>
public sealed record GamePrice(
    int GameId,
    int SteamAppId,
    string Region,
    string? Currency,
    decimal? CurrentRetail,
    decimal? CurrentKeyshops,
    decimal? HistoricalLowRetail,
    decimal? HistoricalLowKeyshops,
    DealVerdict Verdict,
    string? DealUrl);

/// <summary>Price comparison across multiple platforms for a single game.</summary>
public sealed record GamePriceComparison(
    int GameId,
    int SteamAppId,
    IReadOnlyList<PlatformPrice> Platforms);

/// <summary>Price data from a single platform.</summary>
public sealed record PlatformPrice(
    string Platform,
    decimal? CurrentPrice,
    decimal? HistoricalLow,
    decimal? AveragePrice,
    string? Currency,
    string? Url);
