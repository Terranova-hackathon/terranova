using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

using Lune.Auth.Options;
using Lune.Auth.Data;

namespace Lune.Auth.Services;

internal sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    /// <summary>
    /// Creates a JWT token for the specified user. Uses HMAC-SHA256 to sign token. 
    /// To test go here https://codeshack.io/hmac-generator/ 
    /// </summary>
    public string CreateToken(AppUser user)
    {
        //TODO: Implement JWT token creation logic here
                var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.DisplayName ?? user.Email ?? user.Id),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
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