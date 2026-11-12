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
