namespace RevoProfit.Core.Stock.Models;

public record SellOrder
{
    public required DateTime Date { get; init; }
    public required string Ticker { get; init; }
    public required double Amount { get; init; }
    public required double Gains { get; init; }
    public required double FxRate { get; init; }
    public override string ToString()
    {
        return $"{Date}, {Ticker}, ${Math.Round(Amount, 2, MidpointRounding.ToEven)}, ${Math.Round(Gains, 2, MidpointRounding.ToEven)}";
    }
}