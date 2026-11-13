using Lune.Prices.Endpoints;
using Lune.Prices.Options;
using Lune.Prices.Services;
using Lune.Shared.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lune.Prices;

public sealed class PricesModule : IModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GgDealsOptions>(configuration.GetSection(GgDealsOptions.SectionName));
        services.Configure<SteamOptions>(configuration.GetSection(SteamOptions.SectionName));
        services.AddMemoryCache();

        services.AddHttpClient<SteamPriceService>();
        services.AddHttpClient<GgDealsService>();

        services.AddScoped<IPlatformPriceService>(sp => sp.GetRequiredService<SteamPriceService>());
        services.AddScoped<IPlatformPriceService>(sp => sp.GetRequiredService<GgDealsService>());
        services.AddScoped<MultiPlatformPriceService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => PriceEndPoint.Map(endpoints);
}
