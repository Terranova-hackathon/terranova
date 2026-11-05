using Lune.Favorites.Data;
using Lune.Favorites.Services;
using Lune.Shared.Games;
using Lune.Shared.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lune.Favorites.Endpoints;

internal static class FavoritesEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/favorites")
            .WithTags("Favorites")
            .RequireAuthorization();

        // The user always comes from the login token, never from the URL,
        // so nobody can read or change another user's favorites.
        group.MapGet("/", async (HttpContext http, FavoritesService favorites, CancellationToken ct) =>
            http.User.GetUserId() is { } userId
                ? Results.Ok(await favorites.ListAsync(userId, ct))
                : Results.Unauthorized());

        group.MapGet("/ids", async (HttpContext http, FavoritesService favorites, CancellationToken ct) =>
            http.User.GetUserId() is { } userId
                ? Results.Ok(await favorites.IdsAsync(userId, ct))
                : Results.Unauthorized());

        group.MapPut("/{igdbId:int}", Add);
        group.MapDelete("/{igdbId:int}", Remove);
    }

    private static async Task<IResult> Add(
        int igdbId, HttpContext http, FavoritesService favorites, CancellationToken cancellationToken)
    {
        if (http.User.GetUserId() is not { } userId)
        {
            return Results.Unauthorized();
        }

        if (igdbId <= 0)
        {
            return Results.NotFound();
        }

        try
        {
            return await favorites.AddAsync(userId, igdbId, cancellationToken) switch
            {
                AddFavoriteResult.GameNotFound => Results.NotFound(),
                AddFavoriteResult.LimitReached => Results.Problem(
                    title: "Favorites limit reached.",
                    detail: $"You can save up to {FavoriteLimits.MaxPerUser} favorites.",
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.NoContent(),
            };
        }
        catch (GameCatalogUnavailableException ex)
        {
            return Results.Problem(
                title: "Game data is currently unavailable.",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> Remove(
        int igdbId, HttpContext http, FavoritesService favorites, CancellationToken cancellationToken)
    {
        if (http.User.GetUserId() is not { } userId)
        {
            return Results.Unauthorized();
        }

        await favorites.RemoveAsync(userId, igdbId, cancellationToken);
        return Results.NoContent();
    }
}
