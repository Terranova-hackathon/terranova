namespace Lune.Prices.Services;

/// <summary>Raised when the price provider is misconfigured or unreachable.</summary>
public sealed class PricesUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
