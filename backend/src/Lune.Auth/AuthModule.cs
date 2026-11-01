using Lune.Shared.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lune.Auth;
public sealed class AuthModule : IModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add authentication-related services here
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => AuthEndpoints.Map(endpoints);
}
