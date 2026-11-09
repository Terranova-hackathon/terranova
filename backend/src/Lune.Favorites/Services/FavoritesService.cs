using Lune.Favorites.Contracts;
using Lune.Favorites.Data;
using Lune.Shared.Games;
using Lune.Shared.Paging;
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
    public async Task<PagedResult<FavoriteResponse>> ListAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        // IgdbId breaks ties so rows added at the same instant never move between pages.
        var rows = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .ThenBy(f => f.IgdbId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new { f.IgdbId, f.Name, f.BackgroundImage, f.Released, f.Rating, f.Metacritic, f.Genres, f.Platforms, f.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        // SQL Server returns Kind=Unspecified; mark it UTC so the JSON gets a "Z" suffix.
        var items = rows
            .Select(f => new FavoriteResponse(
                f.IgdbId, f.Name, f.BackgroundImage, f.Released, f.Rating, f.Metacritic, f.Genres, f.Platforms,
                DateTime.SpecifyKind(f.CreatedAtUtc, DateTimeKind.Utc)))
            .ToList();

        return new PagedResult<FavoriteResponse>(items, page, pageSize, totalCount);
    }

    /// <summary>Just the game ids, so list pages can draw a filled or empty heart cheaply.</summary>
    public async Task<IReadOnlyList<int>> IdsAsync(string userId, CancellationToken cancellationToken = default) =>
        await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.IgdbId)
            .ToListAsync(cancellationToken);

    /// <summary>Idempotent: favoriting something that is already a favorite succeeds without changes.</summary>
    public async Task<AddFavoriteResult> AddAsync(string userId, int igdbId, CancellationToken cancellationToken = default)
    {
        if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.IgdbId == igdbId, cancellationToken))
        {
            return AddFavoriteResult.AlreadyFavorite;
        }

        if (await db.Favorites.CountAsync(f => f.UserId == userId, cancellationToken) >= FavoriteLimits.MaxPerUser)
        {
            return AddFavoriteResult.LimitReached;
        }

        // The snapshot comes from our own catalog, never from the request, so clients cannot store arbitrary values.
        var game = await catalog.FindAsync(igdbId, cancellationToken);
        if (game is null)
        {
            return AddFavoriteResult.GameNotFound;
        }

        var now = DateTime.UtcNow;
        db.Favorites.Add(new Favorite
        {
            UserId = userId,
            // This is set to Igdb for now but if we wanted to add both rawg we would have to build a separet system to handle both.
            IgdbId = game.Id,
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
            if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.IgdbId == igdbId, cancellationToken))
            {
                return AddFavoriteResult.AlreadyFavorite;
            }

            throw;
        }

        return AddFavoriteResult.Added;
    }

    /// <summary>Idempotent: removing something that is not a favorite is not an error.</summary>
    public Task<int> RemoveAsync(string userId, int igdbId, CancellationToken cancellationToken = default) =>
        db.Favorites
            .Where(f => f.UserId == userId && f.IgdbId == igdbId)
            .ExecuteDeleteAsync(cancellationToken);

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}