using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;


namespace Lune.Auth.Data;

/// <summary>
/// Used only by `dotnet ef` so migrations can be created without starting the whole app.
/// </summary>
internal sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(
                "Server=.;Database=GameHub;",
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.Schema))
            .Options;

        return new AuthDbContext(options);
    }
}
