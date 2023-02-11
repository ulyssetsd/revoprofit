namespace RevoProfit.Core.Stock.Models;

public record StockTransaction
{
    public required DateTime Date { get; init; }
    public required string Ticker { get; init; }
    public required TransactionType Type { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal PricePerShare { get; init; }
    public required decimal TotalAmount { get; init; }
    public required Currency Currency { get; init; }
    public required decimal FxRate { get; init; }
}