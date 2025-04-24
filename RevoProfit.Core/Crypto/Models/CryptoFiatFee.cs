namespace RevoProfit.Core.Crypto.Models;

public record CryptoFiatFee
{
    public required DateTime Date { get; init; }
    public required decimal? FeesInEuros { get; init; }
    public required decimal? FeesInDollars { get; init; }
}