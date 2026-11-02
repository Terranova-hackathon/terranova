using Lune.Auth.Contracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Lune.Auth.Options;

namespace Lune.Auth.Endpoints;

public static class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/logout", Logout);
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();

        //TODO: decide if we want to make add tese end points as well.
        // group.MapPost("/delete-account", DeleteAccount);
        // group.MapPost("/forgot-password", ForgotPassword);
    
        // TODO: Map authentication-related endpoints here
    }

    private static async Task Register(HttpContext context)
    {
        // TODO: Implement registration logic here
        await context.Response.WriteAsync("Register endpoint");
    }

    private static async Task Login(HttpContext context)
    {
        // TODO: Implement login logic here
        await context.Response.WriteAsync("Login endpoint");
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

    // private static async Task DeleteAccount(HttpContext context)
    // {
    //     // TODO: Implement Delete Account logic here
    //     await context.Response.WriteAsync("Delete Account password endpoint");
    // }

    //     private static async Task ForgotPassword(HttpContext context)
    // {
    //     // TODO: Implement Forgot Password logic here
    //     await context.Response.WriteAsync("Forgot password endpoint");
    // }
}


