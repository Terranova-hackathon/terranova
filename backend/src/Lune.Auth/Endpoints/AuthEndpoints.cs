using Lune.Auth.Contracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Lune.Auth.Options;

namespace Lune.Auth.Endpoints;

public static class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", Login);
        group.MapPost("/register", Register);
        group.MapPost("/logout", Logout);
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
    
        // TODO: Map authentication-related endpoints here
    }

    private static async Task Login(HttpContext context)
    {
        // TODO: Implement login logic here
        await context.Response.WriteAsync("Login endpoint");
    }

    private static async Task Register(HttpContext context)
    {
        // TODO: Implement registration logic here
        await context.Response.WriteAsync("Register endpoint");
    }

    private static async Task Logout(HttpContext context)
    {
        // TODO: Implement logout logic here
        await context.Response.WriteAsync("Logout endpoint");
    }

    private static async Task GetCurrentUser(HttpContext context)
    {
        // TODO: Implement logic to get the current authenticated user
        await context.Response.WriteAsync("Current user endpoint");
    }
}


/// <summary>
/// The JWT lives in an HttpOnly cookie so JavaScript (and therefore XSS) can never read it.
/// Any future login method (e.g. GitHub OAuth) should finish by calling <see cref="SetAuthCookie"/>.
/// </summary>
internal static class AuthCookies
{
    /// <summary>
    /// internal method to set the authentication cookie with the specified JWT token.
    /// </summary>
    public static void SetAuthCookie(HttpContext http, JwtOptions options, string token)
    {
        http.Response.Cookies.Append(options.CookieName, token, CreateCookieOptions(http, options));
    }
    /// <summary>
    /// internal method to clear the authentication cookie.
    /// </summary>
    public static void ClearAuthCookie(HttpContext http, JwtOptions options)
    {
        http.Response.Cookies.Delete(options.CookieName, CreateCookieOptions(http, options));
    }

    /// <summary>
    /// internal method to create cookie options for the authentication cookie.
    /// </summary>
    private static CookieOptions CreateCookieOptions(HttpContext http, JwtOptions options) => new()
    {
        HttpOnly = true,
        Secure = http.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddMinutes(options.ExpirationMinutes),
        Path = "/",
    };
}