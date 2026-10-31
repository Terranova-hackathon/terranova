using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace Lune.Shared.Modules;

/// <summary>
/// Defines the contract for a feature module in the application.
/// Basically the shape of a module that the host can interact with.
/// </summary>
public interface IModule
{
    void AddServices(IServiceCollection services, IConfiguration configuration);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}