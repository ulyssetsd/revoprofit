namespace RevoProfit.Core.Crypto.Models;

public record CryptoSell
{
    public required DateTime Date { get; init; }
    public required string Symbol { get; init; }
    public required decimal Amount { get; init; }
    public required decimal AmountInEuros { get; init; }
    public required decimal GainsInEuros { get; init; }
    public required decimal Price { get; init; }
}