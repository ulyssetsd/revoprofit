namespace RevoProfit.Core.Crypto.Models;

public record CryptoReport
{
    public required int Year { get; init; }
    public required decimal GainsInEuros { get; init; }
    public required decimal FeesInEuros { get; init; }
    public required decimal FeesInDollars { get; init; }
}