using System.Security.Claims;

namespace Lune.Shared.Identity;

/// <summary>Lets any module ask "who is calling?" from the login token without referencing Auth.</summary>
public static class UserClaims
{
    /// <summary>The JWT "sub" claim that Auth sets to the user's id.</summary>
    public const string UserId = "sub";

    public static string? GetUserId(this ClaimsPrincipal principal) => principal.FindFirstValue(UserId);
}
