using System.Collections.Concurrent;

namespace MockApi.Shared;

// Maps the opaque Lune_auth cookie value to a user id. Sessions live only in memory.
public class SessionStore
{
    public const string CookieName = "Lune_auth";

    private readonly ConcurrentDictionary<string, string> _sessions = new();

    public void SignIn(HttpContext http, string userId)
    {
        var token = Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Convert.ToHexString(Guid.NewGuid().ToByteArray());
        _sessions[token] = userId;
        http.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Secure = http.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddMinutes(60)
        });
    }

    public void SignOut(HttpContext http)
    {
        if (http.Request.Cookies.TryGetValue(CookieName, out var token))
            _sessions.TryRemove(token, out _);
        http.Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
    }

    public string? GetUserId(HttpContext http) =>
        http.Request.Cookies.TryGetValue(CookieName, out var token) && _sessions.TryGetValue(token, out var id)
            ? id
            : null;
}
