using Lune.Favorites.Data;
using Lune.Shared.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lune.Favorites.Services;

internal sealed class FavoritesDataCleaner(FavoritesDbContext db) : IUserDataCleaner
{
    public Task DeleteUserDataAsync(string userId, CancellationToken cancellationToken = default) =>
        db.Favorites.Where(f => f.UserId == userId).ExecuteDeleteAsync(cancellationToken);
}
