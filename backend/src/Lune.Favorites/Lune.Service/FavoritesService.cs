using Lune.Favorites.Contracts;
using Lune.Favorites.Data;
using Lune.Shared.Games;
using Microsoft.EntityFrameworkCore;

namespace Lune.Favorites.Services;

public enum AddFavoriteResult
{
    Added,
    AlreadyFavorite,
    GameNotFound,
    LimitReached,
}

internal sealed class FavoritesService(FavoritesDbContext db, IGameCatalog catalog)
{
    public const int MaxFavorites = 1000;
    public async Task<IReadOnlyList<FavoriteResponse>> ListAsync(string userId, CancellationToken cancellationToken = default)
    {
        var rows = await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Take(FavoriteLimits.MaxPerUser)
            .Select(f => new { f.RawgId, f.Name, f.BackgroundImage, f.Released, f.Rating, f.Metacritic, f.Genres, f.Platforms, f.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        // SQL Server returns Kind=Unspecified; mark it UTC so the JSON gets a "Z" suffix.
        return rows
            .Select(f => new FavoriteResponse(
                f.RawgId, f.Name, f.BackgroundImage, f.Released, f.Rating, f.Metacritic, f.Genres, f.Platforms,
                DateTime.SpecifyKind(f.CreatedAtUtc, DateTimeKind.Utc)))
            .ToList();
    }

    /// <summary>Just the game ids, so list pages can draw a filled or empty heart cheaply.</summary>
    public async Task<IReadOnlyList<int>> IdsAsync(string userId, CancellationToken cancellationToken = default) =>
        await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.RawgId)
            .ToListAsync(cancellationToken);

    /// <summary>Idempotent: favoriting something that is already a favorite succeeds without changes.</summary>
    public async Task<AddFavoriteResult> AddAsync(string userId, int rawgId, CancellationToken cancellationToken = default)
    {
        if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.RawgId == rawgId, cancellationToken))
        {
            return AddFavoriteResult.AlreadyFavorite;
        }

        if (await db.Favorites.CountAsync(f => f.UserId == userId, cancellationToken) >= FavoriteLimits.MaxPerUser)
        {
            return AddFavoriteResult.LimitReached;
        }

        // The snapshot comes from our own catalog, never from the request, so clients cannot store arbitrary values.
        var game = await catalog.FindAsync(rawgId, cancellationToken);
        if (game is null)
        {
            return AddFavoriteResult.GameNotFound;
        }

        var now = DateTime.UtcNow;
        db.Favorites.Add(new Favorite
        {
            UserId = userId,
            RawgId = game.Id,
            Name = Truncate(game.Name, FavoriteLimits.NameLength),
            BackgroundImage = game.BackgroundImage is { Length: <= FavoriteLimits.BackgroundImageLength } ? game.BackgroundImage : null,
            Released = game.Released is { } released ? Truncate(released, FavoriteLimits.ReleasedLength) : null,
            Rating = game.Rating,
            Metacritic = game.Metacritic,
            Genres = [.. game.Genres],
            Platforms = [.. game.Platforms],
            CreatedAtUtc = now,
            SnapshotUpdatedAtUtc = now,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two requests raced and the other one won; the primary key kept it to a single row.
            db.ChangeTracker.Clear();
            if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.RawgId == rawgId, cancellationToken))
            {
                return AddFavoriteResult.AlreadyFavorite;
            }

            throw;
        }

        return AddFavoriteResult.Added;
    }

    /// <summary>Idempotent: removing something that is not a favorite is not an error.</summary>
    public Task<int> RemoveAsync(string userId, int rawgId, CancellationToken cancellationToken = default) =>
        db.Favorites
            .Where(f => f.UserId == userId && f.RawgId == rawgId)
            .ExecuteDeleteAsync(cancellationToken);

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}