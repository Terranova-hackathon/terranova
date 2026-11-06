using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Lune.Games.Contracts;
using Lune.Games.Services;

namespace Lune.Games.EndPoints;

public static  partial class GameEndPoint
{
    
    private const int MaxPageSize = 50;
    private const int MaxValuePerPage = 20;

    //TODO: implement SlugPattern validation utility function.

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        //TODO: require authentication for game endpoints.
        var group = endpoints.MapGroup("/api/games").WithTags("Games");

        group.MapGet("/", Search);
        group.MapGet("/{id:int}", GetById);
        group.MapGet("/filters", GetFilters);
        // group.MapGet("/companies", SearchCompanies);

    }

    private static async Task<IResult> Search(
    IGamesService games, string? search, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        try
        {
            return Results.Ok(await games.SearchAsync(search, page, pageSize, null, ct));
        }
        catch (GamesUnavailableException e)
        {
            return Unavailable(e);
        }
    }

    private static async Task<IResult> GetById(int id, IGamesService games, CancellationToken ct)
    {
        try
        {
            var game = await games.GetByIdAsync(id, ct);
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

    /// <summary>
    /// Search end point for the all the developer and publishers. 
    /// </summary>
    // private static async Task<IResult> SearchCompanies(
    //     IGamesService games, CancellationToken cancellationToken, string? kind = null, string? search = null)
    // {
    //     ///TODO: add search company method here. 
    // }

    private static IResult Unavailable(GamesUnavailableException e) => 
        Results.Problem(            
            title: "Game data is currently unavailable.",
            detail: e.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
}
