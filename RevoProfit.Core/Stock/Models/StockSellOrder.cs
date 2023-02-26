namespace RevoProfit.Core.Stock.Models;

public record StockSellOrder
{
    public required DateTime Date { get; init; }
    public required string Ticker { get; init; }
    public required decimal Amount { get; init; }
    public required decimal Gains { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal GainsInEuros { get; init; }
}