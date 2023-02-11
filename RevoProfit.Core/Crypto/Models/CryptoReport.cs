namespace RevoProfit.Core.Crypto.Models;

public record CryptoReport
{
    public required int Year { get; init; }
    public required decimal GainsEnEuros { get; init; }
    public required decimal FraisEnEuros { get; init; }
}