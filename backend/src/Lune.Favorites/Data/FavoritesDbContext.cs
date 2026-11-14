using Lune.Shared.Games.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

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

        var filterOptionListComparer = new ValueComparer<List<FilterOption>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            v => v.ToList());

        builder.Entity<Favorite>(favorite =>
        {
            favorite.HasKey(f => new { f.UserId, f.IgdbId });
            favorite.Property(f => f.UserId).HasMaxLength(450);
            favorite.Property(f => f.Name).HasMaxLength(FavoriteLimits.NameLength).IsRequired();
            favorite.Property(f => f.BackgroundImage).HasMaxLength(FavoriteLimits.BackgroundImageLength);
            favorite.Property(f => f.Released).HasMaxLength(FavoriteLimits.ReleasedLength);

            // Genres and Platforms are stored as JSON arrays of FilterOption in an nvarchar(max) column.
            favorite.Property(f => f.Genres)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<FilterOption>>(v, (JsonSerializerOptions?)null) ?? new())
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(filterOptionListComparer);
            favorite.Property(f => f.Platforms)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<FilterOption>>(v, (JsonSerializerOptions?)null) ?? new())
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(filterOptionListComparer);

            // Answers "how many people favorited this game?" for future social features.
            favorite.HasIndex(f => f.IgdbId);
        });
    }
}
