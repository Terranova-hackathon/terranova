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

    }

    private static async Task<IResult> Search(
    IGamesService games, string? search, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return Results.Ok(await games.SearchAsync(search, page, pageSize, null, ct));
    }
}