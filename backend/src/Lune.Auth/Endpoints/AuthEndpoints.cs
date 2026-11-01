using Lune.Auth.Contracts;
using Lune.Shared.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Lune.Auth.Endpoints;

public static class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", Login);
        group.MapPost("/register", Register);
        group.MapPost("/logout", Logout);
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
    
        // TODO: Map authentication-related endpoints here
    }

    private static async Task Login(HttpContext context)
    {
        // TODO: Implement login logic here
        await context.Response.WriteAsync("Login endpoint");
    }

    private static async Task Register(HttpContext context)
    {
        // TODO: Implement registration logic here
        await context.Response.WriteAsync("Register endpoint");
    }

    private static async Task Logout(HttpContext context)
    {
        // TODO: Implement logout logic here
        await context.Response.WriteAsync("Logout endpoint");
    }

    private static async Task GetCurrentUser(HttpContext context)
    {
        // TODO: Implement logic to get the current authenticated user
        await context.Response.WriteAsync("Current user endpoint");
    }
}