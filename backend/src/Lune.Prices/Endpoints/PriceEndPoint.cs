using Lune.Prices.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lune.Prices.Endpoints;

public static class PriceEndPoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/prices").WithTags("Prices");

        group.MapGet("/{Id:int}", GetPriceComparison);
    }

    private static async Task<IResult> GetPriceComparison(
        int Id, MultiPlatformPriceService priceComparison, CancellationToken cancellationToken)
    {
        try
        {
            var comparison = await priceComparison.GetPriceComparisonAsync(Id, cancellationToken);
            if (comparison is null)
                return Results.Problem(
                    title: "No price data found.",
                    detail: "Pricing data is not available for this game yet.",
                    statusCode: StatusCodes.Status404NotFound);

            return Results.Ok(comparison);
        }
        catch (PricesUnavailableException ex)
        {
            return Unavailable(ex);
        }
    }

    private static IResult Unavailable(PricesUnavailableException ex) =>
        Results.Problem(
            title: "Price data is currently unavailable.",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
}

