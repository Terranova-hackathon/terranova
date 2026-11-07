using System.Net;
using System.Text;
using Microsoft.AspNetCore.Routing;

namespace Lune.Api;

/// <summary>
/// THIS PAGE IS AI-GENERATED!
/// Dev landing page on "/" that lists every registered endpoint and offers simple register/login forms.
/// </summary>
public static class LandingPage
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/", (EndpointDataSource source) =>
        {
            var rows = new StringBuilder();

            var endpoints = source.Endpoints
                .OfType<RouteEndpoint>()
                .Where(e => e.RoutePattern.RawText is { } raw && raw != "/" && !raw.StartsWith("/openapi"))
                .SelectMany(e => (e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? ["ANY"])
                    .Select(method => (Method: method, Route: e.RoutePattern.RawText!, Auth: e.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>() is not null)))
                .OrderBy(e => e.Route)
                .ThenBy(e => e.Method);

            foreach (var (method, route, auth) in endpoints)
            {
                rows.Append($"<tr><td><span class=\"m {method}\">{method}</span></td>")
                    .Append($"<td><code>{WebUtility.HtmlEncode(route)}</code></td>")
                    .Append($"<td>{(auth ? "🔒 auth" : "")}</td></tr>");
            }

            return Results.Content(Html.Replace("{{ROWS}}", rows.ToString()), "text/html; charset=utf-8");
        }).ExcludeFromDescription();
    }

    private const string Html = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>Lune API</title>
<style>
  body { font-family: system-ui, sans-serif; max-width: 760px; margin: 2rem auto; padding: 0 1rem; }
  header { display: flex; justify-content: space-between; align-items: center; }
  button { padding: .5rem 1rem; margin-left: .5rem; cursor: pointer; }
  table { width: 100%; border-collapse: collapse; margin-top: 1rem; }
  td { padding: .4rem; border-bottom: 1px solid #ddd; }
  .m { font-weight: bold; font-size: .8rem; padding: 2px 6px; border-radius: 4px; color: #fff; background: #666; }
  .GET { background: #2e7d32; } .POST { background: #1565c0; } .PUT { background: #ef6c00; } .DELETE { background: #c62828; }
  dialog form { display: flex; flex-direction: column; gap: .5rem; min-width: 260px; }
  pre { background: #f4f4f4; padding: .5rem; white-space: pre-wrap; }
</style>
</head>
<body>
<header>
  <h1>Lune API</h1>
  <div>
    <button id="registerBtn">Register</button>
    <button id="loginBtn">Login</button>
    <button id="logoutBtn">Logout</button>
  </div>
</header>

<table>{{ROWS}}</table>
<pre id="out" hidden></pre>

<dialog id="registerDlg">
  <form id="registerForm" method="dialog">
    <h3>Register</h3>
    <input name="email" type="email" placeholder="Email" required>
    <input name="displayName" placeholder="Display name">
    <input name="password" type="password" placeholder="Password (min 8)" required minlength="8">
    <button type="submit">Create account</button>
    <button type="button" data-close>Cancel</button>
  </form>
</dialog>

<dialog id="loginDlg">
  <form id="loginForm" method="dialog">
    <h3>Login</h3>
    <input name="email" type="email" placeholder="Email" required>
    <input name="password" type="password" placeholder="Password" required>
    <button type="submit">Log in</button>
    <button type="button" data-close>Cancel</button>
  </form>
</dialog>

<script>
  const out = document.getElementById('out');

  async function send(url, form, dialog) {
    const body = Object.fromEntries(new FormData(form));
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    out.hidden = false;
    out.textContent = res.status + ' ' + res.statusText + '\n' + await res.text();
    if (res.ok) dialog.close();
  }

  for (const kind of ['register', 'login']) {
    const dlg = document.getElementById(kind + 'Dlg');
    document.getElementById(kind + 'Btn').onclick = () => dlg.showModal();
    document.getElementById(kind + 'Form').onsubmit = e => {
      e.preventDefault();
      send('/api/auth/' + kind, e.target, dlg);
    };
  }
  document.getElementById('logoutBtn').onclick = async () => {
    const res = await fetch('/api/auth/logout', { method: 'POST' });
    out.hidden = false;
    out.textContent = res.status + ' ' + res.statusText;
  };
  document.querySelectorAll('[data-close]').forEach(b => b.onclick = () => b.closest('dialog').close());
</script>
</body>
</html>
""";
}
