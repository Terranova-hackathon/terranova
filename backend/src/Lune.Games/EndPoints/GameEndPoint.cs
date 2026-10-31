using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    }

    private static async Task<IResult> Search(
    IGamesService games, string? search, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return Results.Ok(await games.SearchAsync(search, page, pageSize, null, ct));
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
}
