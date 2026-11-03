using Lune.Games;
using Lune.Auth;
using Lune.Favorites;
using Lune.Shared.Modules;

/// <summary>
/// Entry point for the Lune API application.
/// </summary>

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var modules = new IModule[] { new GamesModule(), new AuthModule(), new FavoritesModule() };

foreach (var m in modules) m.AddServices(builder.Services, builder.Configuration);

var app = builder.Build();

foreach (var m in modules) await m.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => "ok");
Lune.Api.LandingPage.Map(app);

foreach (var m in modules) m.MapEndpoints(app);

app.Run();
