using Lune.Games;
using Lune.Shared.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var modules = new IModule[] { new GamesModule() };

foreach (var m in modules) m.AddServices(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => "ok");

foreach (var m in modules) m.MapEndpoints(app);

app.Run();
