using Lune.Prices.Endpoints;
using Lune.Prices.Options;
using Lune.Prices.Services;
using Lune.Shared.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lune.Prices;

public sealed class PricesModule : IModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GgDealsOptions>(configuration.GetSection(GgDealsOptions.SectionName));
        services.Configure<SteamOptions>(configuration.GetSection(SteamOptions.SectionName));
        services.AddMemoryCache();

        services.AddHttpClient<SteamPriceService>();
        services.AddScoped<IPlatformPriceService>(sp => sp.GetRequiredService<SteamPriceService>());

        services.AddHttpClient<GgDealsService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<GgDealsOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });
        services.AddScoped<IPlatformPriceService>(sp => sp.GetRequiredService<GgDealsService>());

        services.AddScoped<MultiPlatformPriceService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => PriceEndPoint.Map(endpoints);
}

