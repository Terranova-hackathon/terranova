namespace Lune.Prices.Options;

internal sealed class GgDealsOptions
{
    public const string SectionName = "GgDeals";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.gg.deals/v1/";
    // Locked to Italy for now. Region and currency are part of every cache key so a per-user region can be added later.
    public string Region { get; set; } = "it";
    public int CacheMinutes { get; set; } = 360;
}
