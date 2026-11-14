using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Lune.Shared.Games.Contracts;
using Lune.Games.Services;

namespace Lune.Games.EndPoints;

public static class GameEndPoint
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 40;

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/games").WithTags("Games");

        group.MapGet("/", Search)
            .WithName("Main Search")
            .WithDescription("Search for games here with or without search parameters")
            .Accepts<string>("query")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/{id:int}", GetById)
            .WithName("Get Game By ID")
            .WithDescription("Retrieve a game by its ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/filters", GetFilters)
            .WithName("Get Filter Options")
            .WithDescription("Retrieve all available filter options for games")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/companies", SearchCompanies)
            .WithName("Search Companies")
            .WithDescription("Search for game companies")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/featured", GetFeatured)
            .WithName("Get Featured Games")
            .WithDescription("Retrieve featured games")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> Search(
        IGamesService games,
        string? search,
        int page = 1,
        int pageSize = DefaultPageSize,
        string? genres = null,
        string? platforms = null,
        string? developers = null,
        string? publishers = null,
        string? ordering = null,
        string? metacriticMin = null,
        string? metacriticMax = null,
        CancellationToken cancellationToken = default)
    {
        var filters = GameSearchFilterParser.Parse(
            genres, platforms, developers, publishers, ordering, metacriticMin, metacriticMax, out var errors);
        if (filters is null)
            return Results.ValidationProblem(errors);

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        try
        {
            return Results.Ok(await games.SearchAsync(search, page, pageSize, filters, cancellationToken));
        }
        catch (GamesUnavailableException e)
        {
            return Unavailable(e);
        }
    }

    private static async Task<IResult> GetById(
        int id,
        IGamesService games,
        int page = 1,
        int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        try
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var game = await games.GetByIdAsync(id, page, pageSize, cancellationToken);
            return game is null ? Results.NotFound() : Results.Ok(game);
        }
        catch (GamesUnavailableException ex)
        {
            return Results.Problem(
                title: "Game data is currently unavailable.",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Grabs all the filterable options form the third party API so the front end can diplsay all the filter options.
    /// </summary>
        private static async Task<IResult> GetFilters(IGamesService games, CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await games.GetFilterOptionsAsync(cancellationToken));
        }
        catch (GamesUnavailableException e)
        {
            return Unavailable(e);
        }
    }

    private static async Task<IResult> SearchCompanies(
        IGamesService games, CancellationToken cancellationToken, string? kind = null, string? search = null)
    {
        var errors = new Dictionary<string, string[]>();
        var companyKind = kind?.Trim().ToLowerInvariant() switch
        {
            "developer" => CompanyKind.Developer,
            "publisher" => CompanyKind.Publisher,
            _ => (CompanyKind?)null
        };
        if (companyKind is null)
            errors["kind"] = ["kind must be 'developer' or 'publisher'."];

        var term = search?.Trim() ?? string.Empty;
        if (term.Length is < 2 or > 60)
            errors["search"] = ["search must be between 2 and 60 characters."];

        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        try
        {
            return Results.Ok(await games.SearchCompaniesAsync(companyKind!.Value, term, cancellationToken));
        }
        catch (GamesUnavailableException e)
        {
            return Unavailable(e);
        }
    }

    private static async Task<IResult> GetFeatured(
        IGamesService games, CancellationToken cancellationToken)
    {
        try
        {
            var featured = await games.GetFeaturedAsync(cancellationToken);
            return featured.Count == 0 ? Results.NotFound() : Results.Ok(featured);
        }
        catch (GamesUnavailableException e)
        {
            return Unavailable(e);
        }
    }

    private static IResult Unavailable(GamesUnavailableException e) => 
        Results.Problem(            
            title: "Game data is currently unavailable.",
            detail: e.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
}
