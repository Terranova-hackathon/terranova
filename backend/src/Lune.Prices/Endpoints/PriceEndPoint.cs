using Lune.Prices.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lune.Prices.Endpoints;

public static class PriceEndPoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/games/prices").WithTags("Prices");

        group.MapGet("/{gameId:int}", GetByGameId);
        group.MapGet("/{gameId:int}/comparison", GetPriceComparison);
    }

    private static async Task<IResult> GetByGameId(
        int gameId, IPriceService prices, CancellationToken cancellationToken)
    {
        try
        {
            var price = await prices.GetByGameIdAsync(gameId, cancellationToken);
            return price is null
                ? Results.Problem(
                    title: "No price data for this game.",
                    detail: "The game is unknown, has no Steam release, or has no price data yet.",
                    statusCode: StatusCodes.Status404NotFound)
                : Results.Ok(price);
        }
        catch (PricesUnavailableException ex)
        {
            return Results.Problem(
                title: "Price data is currently unavailable.",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> GetPriceComparison(
        int gameId, MultiPlatformPriceService priceComparison, CancellationToken cancellationToken)
    {
        try
        {
            var comparison = await priceComparison.GetPriceComparisonAsync(gameId, cancellationToken);
            return comparison is null
                ? Results.Problem(
                    title: "No price data for this game.",
                    detail: "The game is unknown or has no pricing information across platforms.",
                    statusCode: StatusCodes.Status404NotFound)
                : Results.Ok(comparison);
        }
        catch (PricesUnavailableException ex)
        {
            return Results.Problem(
                title: "Price data is currently unavailable.",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}

