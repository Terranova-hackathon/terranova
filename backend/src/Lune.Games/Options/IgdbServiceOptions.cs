namespace Lune.Games.Options;

internal sealed class IgdbServiceOptions
{
    public const string SectionName = "Igdb";
    public string ClientId { get; set; } = string.Empty;
    public string ApiSecret {get; set;} = string.Empty;
    public string BaseUrl { get; set; } = "https://api.igdb.com/v4/";
    public string TokenUrl { get; set;} = "https://id.twitch.tv/oauth2/token";
    public string ImageBaseUrl {get; set ;} = string.Empty; //https://images.igdb.com/igdb/image/upload
    // The Idgb data base doesnt give me a full url path so i have to build it my self. 
}