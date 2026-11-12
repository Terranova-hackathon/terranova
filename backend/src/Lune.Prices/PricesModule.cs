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
        services.AddMemoryCache();

        // The API key travels in the query string, so keep HttpClient from logging request URLs.
        services.AddHttpClient<IPriceService, GgDealsPriceService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<GgDealsOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
            })
            .RemoveAllLoggers();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => PriceEndPoint.Map(endpoints);
}
