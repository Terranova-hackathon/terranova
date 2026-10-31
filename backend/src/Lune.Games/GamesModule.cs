

using Lune.Shared.Modules;
using Lune.Games.EndPoints;
using Lune.Games.Services;
using Lune.Games.Options;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace Lune.Games;
public sealed class GamesModule : IModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        //TODO: register game-related services here.
        services.Configure<RawgServiceOptions>(configuration.GetSection(RawgServiceOptions.SectionName));

        services.AddHttpClient<IGamesService, RawgGamesService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<RawgServiceOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => GameEndPoint.Map(endpoints);
}