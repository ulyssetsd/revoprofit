namespace RevoProfit.Core.Crypto.Models;

public record CryptoTransaction
{
    public required CryptoTransactionType Type { get; init; }
    public required DateTime Date { get; init; }
    public required decimal BuyAmount { get; init; }
    public required string BuySymbol { get; init; }
    public required decimal BuyPrice { get; init; }
    public required decimal SellAmount { get; init; }
    public required string SellSymbol { get; init; }
    public required decimal SellPrice { get; init; }
    public required decimal FeesAmount { get; init; }
    public required string FeesSymbol { get; init; }
    public required decimal FeesPrice { get; init; }
}