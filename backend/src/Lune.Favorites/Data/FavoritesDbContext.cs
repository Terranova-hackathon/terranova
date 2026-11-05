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
            favorite.HasKey(f => new { f.UserId, f.IgdbId });
            favorite.Property(f => f.UserId).HasMaxLength(450);
            favorite.Property(f => f.Name).HasMaxLength(FavoriteLimits.NameLength).IsRequired();
            favorite.Property(f => f.BackgroundImage).HasMaxLength(FavoriteLimits.BackgroundImageLength);
            favorite.Property(f => f.Released).HasMaxLength(FavoriteLimits.ReleasedLength);

            // Genres and Platforms are stored as JSON arrays in an nvarchar(max) column.
            favorite.PrimitiveCollection(f => f.Genres);
            favorite.PrimitiveCollection(f => f.Platforms);

            // Answers "how many people favorited this game?" for future social features.
            favorite.HasIndex(f => f.IgdbId);
        });
    }
}
