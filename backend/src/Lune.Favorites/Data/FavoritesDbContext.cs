using Microsoft.EntityFrameworkCore;

namespace Lune.Favorites.Data;

/// <summary>
/// Database context for the favorites module. Owns the "favorites" schema.
/// </summary>
public class FavoritesDbContext(DbContextOptions<FavoritesDbContext> options) : DbContext(options)
{
    public const string Schema = "favorites";

    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);

        builder.Entity<Favorite>(favorite =>
        {
            favorite.HasKey(f => new { f.UserId, f.RawgId });
            favorite.Property(f => f.UserId).HasMaxLength(450);
            favorite.Property(f => f.Name).HasMaxLength(300).IsRequired();
            favorite.Property(f => f.ImageUrl).HasMaxLength(500);
            favorite.Property(f => f.Released).HasMaxLength(10);

            // Genres and Platforms are stored as JSON arrays in an nvarchar(max) column.
            favorite.PrimitiveCollection(f => f.Genres);
            favorite.PrimitiveCollection(f => f.Platforms);

            // Answers "how many people favorited this game?" for future social features.
            favorite.HasIndex(f => f.RawgId);
        });
    }
}
