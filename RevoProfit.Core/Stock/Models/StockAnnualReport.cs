namespace RevoProfit.Core.Stock.Models;

public record StockAnnualReport
{
    public required int Year { get; init; }
    public required decimal Gains { get; init; }
    public required decimal Dividends { get; init; }
    public required decimal CashTopUp { get; init; }
    public required decimal CashWithdrawal { get; init; }
    public required decimal CustodyFee { get; init; }

    public required decimal GainsInEuro { get; init; }
    public required decimal DividendsInEuro { get; init; }
    public required decimal CashTopUpInEuro { get; init; }
    public required decimal CashWithdrawalInEuro { get; init; }
    public required decimal CustodyFeeInEuro { get; init; }

    public required IEnumerable<StockSellOrder> StockSellOrders { get; init; }

    public override string ToString()
    {
        return $"{Year}, Gains Realized: ${Math.Round(Gains, 2, MidpointRounding.ToEven)}, Dividends: ${Math.Round(Dividends, 2, MidpointRounding.ToEven)}, CashTopUp: ${Math.Round(CashTopUp, 2, MidpointRounding.ToEven)}, CashWithdrawal: ${Math.Round(CashWithdrawal, 2, MidpointRounding.ToEven)}, CustodyFee: ${Math.Round(CustodyFee, 2, MidpointRounding.ToEven)}\n" +
               $"{Year}, Gains Realized: {Math.Round(GainsInEuro, 2, MidpointRounding.ToEven)} EUR, Dividends: {Math.Round(DividendsInEuro, 2, MidpointRounding.ToEven)} EUR, CashTopUp: {Math.Round(CashTopUpInEuro, 2, MidpointRounding.ToEven)} EUR, CashWithdrawal: {Math.Round(CashWithdrawalInEuro, 2, MidpointRounding.ToEven)} EUR, CustodyFee: ${Math.Round(CustodyFeeInEuro, 2, MidpointRounding.ToEven)} EUR";
    }
}