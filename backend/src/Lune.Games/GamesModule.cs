
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Lune.Shared.Games;
using Lune.Shared.Modules;
using Lune.Games.EndPoints;
using Lune.Games.Services;
using Lune.Games.Options;


namespace Lune.Games;
public sealed class GamesModule : IModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // this registers the rawg game service if we want to swap it out at all. (rawg.io game service is deprecated at this point)
        // services.Configure<RawgServiceOptions>(configuration.GetSection(RawgServiceOptions.SectionName));

        // services.AddHttpClient<IGamesService, RawgGamesService>((sp, client) =>
        // {
        //     var opts = sp.GetRequiredService<IOptions<RawgServiceOptions>>().Value;
        //     client.BaseAddress = new Uri(opts.BaseUrl);
        // });
        
        // This is the new Igdb intigration. old rawgio is deprecated for now but can implemnt later if we decide to add the service. 
        services.Configure<IgdbServiceOptions>(configuration.GetSection(IgdbServiceOptions.SectionName));
        services.AddMemoryCache(); // caches the filter options
        services.AddHttpClient(); // lets IgdbTokenProvider use IHttpClientFactory
        services.AddSingleton<IgdbTokenProvider>();

        services.AddHttpClient<IGamesService, IgdbGamesService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<IgdbServiceOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });

        services.AddScoped<IGameCatalog, GameCatalogAdapter>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => GameEndPoint.Map(endpoints);
}
