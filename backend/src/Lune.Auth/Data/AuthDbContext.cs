
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lune.Auth.Data;

/// <summary>
/// Represents the database context for authentication-related entities.
/// </summary>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) :IdentityDbContext<AppUser>(options)
{
    public const string Schema = "auth";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);   // 1. Put all tables in a folder called "auth"
        base.OnModelCreating(builder);      // 2. Let the built-in login system add its tables
    }
}