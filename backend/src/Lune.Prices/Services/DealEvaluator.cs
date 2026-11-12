using Lune.Prices.Contracts;

namespace Lune.Prices.Services;

internal static class DealEvaluator
{
    /// <summary>Compares the current official retail price with the historical low (ratio of 1.0 means equal to the low).</summary>
    internal static DealVerdict Evaluate(decimal? currentRetail, decimal? historicalLowRetail)
    {
        if (currentRetail is not > 0 || historicalLowRetail is not > 0)
            return DealVerdict.Unknown;

        return (currentRetail.Value / historicalLowRetail.Value) switch
        {
            <= 1.05m => DealVerdict.HistoricalLow,
            <= 1.20m => DealVerdict.GreatDeal,
            <= 1.50m => DealVerdict.GoodDeal,
            <= 2.00m => DealVerdict.Fair,
            _ => DealVerdict.Wait,
        };
    }
}
