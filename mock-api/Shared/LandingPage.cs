using System.Net;
using System.Text;
using Microsoft.AspNetCore.Routing.Patterns;

namespace MockApi.Shared;

public static class LandingPage
{
    private static readonly string[] ProtectedPrefixes = ["/api/games", "/api/favorites", "/api/auth/me"];

    public static void MapLandingPage(this WebApplication app)
    {
        // Built from the registered routes so it never drifts from the real endpoints.
        app.MapGet("/", (EndpointDataSource routes) => Results.Content(Render(routes), "text/html; charset=utf-8"))
            .ExcludeFromDescription();
    }

    private static string Render(EndpointDataSource routes)
    {
        var rows = routes.Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => new
            {
                Path = System.Text.RegularExpressions.Regex.Replace(e.RoutePattern.RawText ?? "", @"\{(\w+):\w+\}", "{$1}").TrimEnd('/') is { Length: > 0 } p ? p : "/",
                Methods = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? []
            })
            .Where(r => r.Path is not "/" && !r.Path.Contains("**"))
            .SelectMany(r => r.Methods.Select(m => (Method: m, r.Path)))
            .OrderBy(r => r.Path == "/health" ? 1 : 0).ThenBy(r => r.Path).ThenBy(r => r.Method)
            .ToList();

        var sb = new StringBuilder();
        sb.Append("""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><title>Lune mock API</title>
            <style>
            body{font:15px/1.5 system-ui,sans-serif;max-width:760px;margin:2rem auto;padding:0 1rem;color:#222}
            table{border-collapse:collapse;width:100%}td,th{text-align:left;padding:.4rem .6rem;border-bottom:1px solid #ddd}
            code{background:#f3f3f3;padding:.1rem .3rem;border-radius:3px}
            .m{font-weight:600;font-family:monospace}.lock{color:#b45309}
            </style></head><body>
            <h1>Lune mock API</h1>
            <p>In-memory mock of the Lune backend. Demo login: <code>demo@example.com</code> / <code>Passw0rd!</code>.
            Add <code>?mock503=1</code> to games requests to simulate the provider being down.
            Full spec: <code>docs/API.md</code>.</p>
            <form id="f" style="margin:1rem 0;display:flex;gap:.5rem;flex-wrap:wrap">
            <input name="email" value="demo@example.com" placeholder="email"><input name="password" type="password" value="Passw0rd!" placeholder="password">
            <button>Log in</button><button type="button" id="out">Log out</button><span id="msg"></span></form>
            <script>
            const msg=document.getElementById('msg');
            document.getElementById('f').onsubmit=async e=>{e.preventDefault();const d=new FormData(e.target);
            const r=await fetch('/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email:d.get('email'),password:d.get('password')})});
            msg.textContent=r.ok?'Logged in as '+(await r.json()).email:'Login failed ('+r.status+')'};
            document.getElementById('out').onclick=async()=>{await fetch('/api/auth/logout',{method:'POST',headers:{'Content-Type':'application/json'},body:'{}'});msg.textContent='Logged out'};
            </script>
            <table><tr><th>Method</th><th>Route</th><th>Auth</th></tr>
            """);

        foreach (var (method, path) in rows)
        {
            var needsAuth = ProtectedPrefixes.Any(p => path.StartsWith(p, StringComparison.Ordinal));
            var shown = WebUtility.HtmlEncode(path);
            var link = method == "GET" && !path.Contains('{') ? $"<a href=\"{shown}\">{shown}</a>" : shown;
            sb.Append($"<tr><td class=\"m\">{method}</td><td>{link}</td><td>{(needsAuth ? "<span class=\"lock\">cookie</span>" : "-")}</td></tr>");
        }

        sb.Append("</table></body></html>");
        return sb.ToString();
    }
}
