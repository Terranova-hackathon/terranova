namespace Lune.Prices.Options;

internal sealed class SteamOptions
{
    public const string SectionName = "Steam";
    /// <summary>
    /// also locked to italy for now. 
    /// </summary>
    public string CurrencyCode { get; set; } = "it";
    public int CacheMinutes { get; set; } = 360;
    public string BaseUrl { get; set; } = "https://store.steampowered.com/app";
    public string SteamApiUrlPath { get; set; } =  "https://store.steampowered.com/api/appdetails";
    
}
