using Lune.Favorites.Data;
using Lune.Favorites.Endpoints;
using Lune.Shared.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lune.Favorites;

public sealed class FavoritesModule : IModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FavoritesDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", FavoritesDbContext.Schema)));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => FavoritesEndpoints.Map(endpoints);

    public async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FavoritesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<FavoritesModule>();

        // SQL Server in Docker can take a while to accept connections, so retry.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 15)
            {
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}), retrying in 4s...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
            }
        }
    }
}
