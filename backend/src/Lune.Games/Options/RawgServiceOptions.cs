namespace Lune.Games.Options;

internal sealed class RawgServiceOptions
{
    public const string SectionName = "Rawg";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.rawg.io/api/";
    public int CacheMinutes { get; set; } = 10;
}