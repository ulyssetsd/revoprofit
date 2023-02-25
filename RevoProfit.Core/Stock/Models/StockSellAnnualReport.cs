namespace RevoProfit.Core.Stock.Models;

public record StockSellAnnualReport
{
    public required IEnumerable<StockSellOrder> StockSellOrders { get; init; }
    public required decimal Gains { get; init; }
    public required decimal GainsInEuro { get; init; }
}