namespace Lune.Games.Services;

internal static class IgdbGameFields
{
    public const string Summary =
        "id,name,first_release_date,cover.image_id,total_rating,aggregated_rating,genres.name,platforms.name";

    public const string Detail =
        Summary + ",summary,storyline,url,websites.url,artworks.image_id,screenshots.image_id,videos.video_id,videos.name,similar_games,external_games.uid,external_games.url,external_games.external_game_source.name,involved_companies.developer,involved_companies.publisher,involved_companies.company.name";

    // IGDB lists 200+ platforms, most of them retro or obscure. The filter dropdown offers:
    // PC (Windows), Mac, Linux, PS5, PS4, PS3, Xbox Series X|S, Xbox One, Xbox 360,
    // Switch 2, Switch, Wii U, Wii, 3DS, Android, and iOS.
    public const string PlatformIds = "6,14,3,167,48,9,169,49,12,508,130,41,5,37,34,39";
}
