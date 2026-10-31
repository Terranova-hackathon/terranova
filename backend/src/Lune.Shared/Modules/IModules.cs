using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Defines the contract for a feature module in the application.
/// Basically the shape of a module that the host can interact with.
/// </summary>

namespace Lune.Shared.Modules;

public interface IModule
{
    void AddServices(IServiceCollection services, IConfiguration configuration);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}