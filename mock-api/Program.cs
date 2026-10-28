using System.Text.Json;
using MockApi.Auth;
using MockApi.Favorites;
using MockApi.Games;
using MockApi.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<GameCatalog>();
builder.Services.AddSingleton<FavoriteStore>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(ctx =>
    Problems.Problem(500, "An unexpected error occurred.").ExecuteAsync(ctx)));

app.MapGet("/health", () => Results.Text("Healthy"));

app.MapAuthEndpoints();
app.MapGamesEndpoints();
app.MapFavoritesEndpoints();
app.MapLandingPage();

// Unknown API routes get an empty 404.
app.Map("/api/{**rest}", () => Results.NotFound());

app.Run();
