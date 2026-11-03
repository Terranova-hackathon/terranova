using System.Security.Claims;
using Lune.Favorites.Contracts;
using Lune.Favorites.Data;
using Lune.Shared.Games;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Lune.Favorites.Endpoints;

public static class FavoritesEndpoints
{
    private const int MaxFavorites = 1000;
    private const int MaxNameLength = 300;
    private const int MaxImageUrlLength = 500;
    private const int MaxReleasedLength = 10;

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/favorites").WithTags("Favorites").RequireAuthorization();

        group.MapGet("/", List);
        group.MapGet("/ids", ListIds);
        group.MapPut("/{rawgId:int}", Add);
        group.MapDelete("/{rawgId:int}", Remove);
    }

    // JWT inbound claim mapping is disabled, so the user id stays in the "sub" claim.
    private static string? GetUserId(ClaimsPrincipal principal) => principal.FindFirstValue("sub");

    private static async Task<IResult> List(ClaimsPrincipal principal, FavoritesDbContext db, CancellationToken ct)
    {
        if (GetUserId(principal) is not { } userId) return Results.Unauthorized();

        var rows = await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Take(MaxFavorites)
            .Select(f => new { f.RawgId, f.Name, f.ImageUrl, f.Released, f.Rating, f.Metacritic, f.Genres, f.Platforms, f.CreatedAtUtc })
            .ToListAsync(ct);

        // SQL Server returns Kind=Unspecified; mark it UTC so the JSON gets a "Z" suffix.
        return Results.Ok(rows.Select(f => new FavoriteResponse(
            f.RawgId, f.Name, f.ImageUrl, f.Released, f.Rating, f.Metacritic, f.Genres, f.Platforms,
            DateTime.SpecifyKind(f.CreatedAtUtc, DateTimeKind.Utc))));
    }

    private static async Task<IResult> ListIds(ClaimsPrincipal principal, FavoritesDbContext db, CancellationToken ct)
    {
        if (GetUserId(principal) is not { } userId) return Results.Unauthorized();

        var ids = await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => f.RawgId)
            .ToListAsync(ct);

        return Results.Ok(ids);
    }

    private static async Task<IResult> Add(
        int rawgId, ClaimsPrincipal principal, FavoritesDbContext db, IGameCatalog catalog, CancellationToken ct)
    {
        if (GetUserId(principal) is not { } userId) return Results.Unauthorized();
        if (rawgId <= 0) return Results.NotFound();

        // Idempotent: an existing favorite succeeds without a catalog lookup or a limit check.
        if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.RawgId == rawgId, ct))
            return Results.NoContent();

        if (await db.Favorites.CountAsync(f => f.UserId == userId, ct) >= MaxFavorites)
            return Results.Problem(title: "Favorites limit reached.", statusCode: StatusCodes.Status409Conflict);

        GameSnapshot? game;
        try
        {
            game = await catalog.GetSnapshotAsync(rawgId, ct);
        }
        catch (GameCatalogUnavailableException ex)
        {
            return Results.Problem(
                title: "Game data is currently unavailable.",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (game is null) return Results.NotFound();

        var now = DateTime.UtcNow;
        db.Favorites.Add(new Favorite
        {
            UserId = userId,
            RawgId = rawgId,
            Name = Truncate(game.Name, MaxNameLength),
            ImageUrl = game.ImageUrl is { Length: <= MaxImageUrlLength } image ? image : null,
            Released = game.Released is { } released ? Truncate(released, MaxReleasedLength) : null,
            Rating = game.Rating,
            Metacritic = game.Metacritic,
            Genres = [.. game.Genres],
            Platforms = [.. game.Platforms],
            CreatedAtUtc = now,
            SnapshotUpdatedAtUtc = now,
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A concurrent request may have inserted the same (UserId, RawgId); that still counts as success.
            if (!await db.Favorites.AsNoTracking().AnyAsync(f => f.UserId == userId && f.RawgId == rawgId, ct))
                throw;
        }

        return Results.NoContent();
    }

    private static async Task<IResult> Remove(
        int rawgId, ClaimsPrincipal principal, FavoritesDbContext db, CancellationToken ct)
    {
        if (GetUserId(principal) is not { } userId) return Results.Unauthorized();

        await db.Favorites.Where(f => f.UserId == userId && f.RawgId == rawgId).ExecuteDeleteAsync(ct);
        return Results.NoContent();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
