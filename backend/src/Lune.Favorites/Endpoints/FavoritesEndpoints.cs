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
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 40;

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/favorites")
            .WithTags("Favorites");

        group.MapGet("/", List);
        group.MapGet("/ids", async (HttpContext http, FavoritesService favorites, CancellationToken cancellationToken) =>
            http.User.GetUserId() is { } userId
                ? Results.Ok(await favorites.IdsAsync(userId, cancellationToken))
                : Results.Unauthorized());
        group.MapPut("/{Id:int}", Add);
        group.MapDelete("/{Id:int}", Remove);
    }

    private static async Task<IResult> List(
        HttpContext http,
        FavoritesService favorites,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        if (http.User.GetUserId() is not { } userId)
        {
            return Results.Unauthorized();
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        return Results.Ok(await favorites.ListAsync(userId, page, pageSize, cancellationToken));
    }

    private static async Task<IResult> Add(
        int Id, HttpContext http, FavoritesService favorites, CancellationToken cancellationToken)
    {
        if (http.User.GetUserId() is not { } userId)
        {
            return Results.Unauthorized();
        }

        if (Id <= 0)
        {
            return Results.NotFound();
        }

        try
        {
            return await favorites.AddAsync(userId, Id, cancellationToken) switch
            {
                AddFavoriteResult.GameNotFound => Results.NotFound(),
                AddFavoriteResult.LimitReached => Results.Problem(
                    title: "Favorites limit reached.",
                    detail: $"You can save up to {FavoriteLimits.MaxPerUser} favorites.",
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.Created(),
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
        int Id, HttpContext http, FavoritesService favorites, CancellationToken cancellationToken)
    {
        if (http.User.GetUserId() is not { } userId)
        {
            return Results.Unauthorized();
        }

        await favorites.RemoveAsync(userId, Id, cancellationToken);
        return Results.NoContent();
    }
}
