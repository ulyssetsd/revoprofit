namespace RevoProfit.Core.Stock.Models;

public record StockTransaction
{
    public required DateTime Date { get; init; }
    public required string Ticker { get; init; }
    public required TransactionType Type { get; init; }
    public required double Quantity { get; init; }
    public required double PricePerShare { get; init; }
    public required double TotalAmount { get; init; }
    public required Currency Currency { get; init; }
    public required double FxRate { get; init; }
}