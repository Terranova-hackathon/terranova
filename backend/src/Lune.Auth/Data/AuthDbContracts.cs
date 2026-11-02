using Microsoft.AspNetCore.Identity;

/// <summary>
/// Represents the application user entity that we use to manage authentication and user information internally.
/// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityuser?view=aspnetcore-10.0
/// IdentityUser provides the basic properties for user management such as Id, UserName, Email, and PasswordHash.
/// AppUser extends IdentityUser to include additional properties like DisplayName and CreatedAtUtc.
/// </summary>

namespace Lune.Auth.Data;
public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}