using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Lune.Auth.Contracts;
using Lune.Auth.Options;
using Lune.Auth.Data;
using Lune.Auth.Services;
using Lune.Shared.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
    }

    /// <summary>
    /// these end points only do jwt for now, will need to make a new genaric interface and add the other Services like OAuth if we want to add other login methods. 
    /// </summary>
    private static async Task<IResult> Register(
        RegisterRequest req, 
        UserManager<AppUser> users, 
        JwtTokenService tokens, 
        IOptions<JwtOptions> jwtOptions, 
        HttpContext context)
    {
        if(!RequestValidator.TryValidate(req, out var errors))
        {
            return Results.ValidationProblem(errors);
        }
        
        var user = new AppUser
        {
            Email = req.Email, 
            UserName = req.Email,
            DisplayName = req.DisplayName,
        };

        var res = await users.CreateAsync(user, req.Password);

        ///<summary>
        /// Groups erros together by their code so no duplicate error messages are returned.
        /// </summary>
        if(!res.Succeeded)
        {
            return Results.ValidationProblem(res.Errors.GroupBy(e => e.Code).ToDictionary(
                group => group.Key,
                group => group.Select(e => e.Description).ToArray()
            ));
        }

        AuthCookies.SetAuthCookie(context, jwtOptions.Value, tokens.CreateToken(user));
        return Results.Created("/api/auth/me", MakeResponse(user));
    }

        ///<summary>
        /// Simple Auth for getting the user and trying to validate the password.
        /// we can addd more validations  in the future if needed. eg. login attempts tracking.
        /// </summary>
    private static async Task<IResult> Login(
        LoginRequest req,
        UserManager<AppUser> users,
        JwtTokenService tokens,
        IOptions<JwtOptions> jwtOptions,
        HttpContext context)
    {
        if(!RequestValidator.TryValidate(req, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var user = await users.FindByEmailAsync(req.Email);
        if(user == null || !await users.CheckPasswordAsync(user, req.Password))
        {
            return InvalidCredentials();
        }

        AuthCookies.SetAuthCookie(context, jwtOptions.Value, tokens.CreateToken(user));
        return Results.Ok(MakeResponse(user));
    }


    private static async Task<IResult> Logout( IOptions<JwtOptions> jwtOptions, HttpContext context)
    {
        AuthCookies.ClearAuthCookie(context, jwtOptions.Value);
        return await Task.FromResult(Results.NoContent());
    }


    /// <summary>
    /// principal representing the current user. FindFIrstValue is used to get the user ID from the JWT token.
    /// JwtRegisteredClaimNames.Sub is used to get the user ID from the JWT token.
    /// Gets the current authenticated user based on the JWT token in the request.
    /// </summary>
    private static async Task<IResult> GetCurrentUser(ClaimsPrincipal principal, UserManager<AppUser> users)
    {
        var id = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if( id == null)
            return Results.Unauthorized();

        var user = await users.FindByIdAsync(id);

        if (user == null)
            return Results.Unauthorized();

        return Results.Ok(MakeResponse(user));
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

    internal static IResult InvalidCredentials() =>
        Results.Problem(
            title: "Invalid email or password.",
            statusCode: StatusCodes.Status401Unauthorized);

    internal static UserResponse MakeResponse(AppUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName);
}


