namespace Lune.Prices.Options;

internal sealed class SteamOptions
{
    public const string SectionName = "Steam";
    public string CurrencyCode { get; set; } = "it";
    public int CacheMinutes { get; set; } = 360;
}
