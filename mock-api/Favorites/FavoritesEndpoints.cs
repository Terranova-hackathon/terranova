using MockApi.Games;
using MockApi.Shared;

namespace MockApi.Favorites;

public static class FavoritesEndpoints
{
    public static void MapFavoritesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/favorites").RequireSession();

        group.MapGet("", (HttpContext http, FavoriteStore store) =>
            Results.Ok(store.List(http.CurrentUserId()).Select(r => r.ToDto())));

        group.MapGet("/ids", (HttpContext http, FavoriteStore store) =>
            Results.Ok(store.List(http.CurrentUserId()).Select(r => r.RawgId)));

        group.MapPut("/{rawgId:int}", (int rawgId, HttpContext http, FavoriteStore store, GameCatalog catalog) =>
        {
            if (Problems.Wants503(http.Request)) return Problems.ProviderUnavailable();

            var game = rawgId > 0 ? catalog.Find(rawgId) : null;
            if (game is null) return Results.NotFound();

            var name = game.Name.Length > 300 ? game.Name[..300] : game.Name;
            var image = game.BackgroundImage is { Length: <= 500 } url ? url : null;
            var result = store.Add(new FavoriteRow { UserId = http.CurrentUserId(), RawgId = rawgId, Name = name, ImageUrl = image });

            return result == FavoriteStore.AddResult.LimitReached
                ? Problems.Problem(409, "Favorites limit reached.")
                : Results.NoContent();
        });

        group.MapDelete("/{rawgId:int}", (int rawgId, HttpContext http, FavoriteStore store) =>
        {
            store.Remove(http.CurrentUserId(), rawgId);
            return Results.NoContent();
        });
    }
}
