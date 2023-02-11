namespace RevoProfit.Core.Stock.Models;

public record AnnualReport
{
    public required int Year { get; init; }
    public required double Gains { get; init; }
    public required double Dividends { get; init; }
    public required double CashTopUp { get; init; }
    public required double CashWithdrawal { get; init; }
    public required double CustodyFee { get; init; }

    public required double GainsInEuro { get; init; }
    public required double DividendsInEuro { get; init; }
    public required double CashTopUpInEuro { get; init; }
    public required double CashWithdrawalInEuro { get; init; }
    public required double CustodyFeeInEuro { get; init; }

    public required IEnumerable<SellOrder> SellOrders { get; init; }

    public override string ToString()
    {
        return $"{Year}, Gains Realized: ${Math.Round(Gains, 2, MidpointRounding.ToEven)}, Dividends: ${Math.Round(Dividends, 2, MidpointRounding.ToEven)}, CashTopUp: ${Math.Round(CashTopUp, 2, MidpointRounding.ToEven)}, CashWithdrawal: ${Math.Round(CashWithdrawal, 2, MidpointRounding.ToEven)}, CustodyFee: ${Math.Round(CustodyFee, 2, MidpointRounding.ToEven)}\n" +
               $"{Year}, Gains Realized: {Math.Round(GainsInEuro, 2, MidpointRounding.ToEven)} EUR, Dividends: {Math.Round(DividendsInEuro, 2, MidpointRounding.ToEven)} EUR, CashTopUp: {Math.Round(CashTopUpInEuro, 2, MidpointRounding.ToEven)} EUR, CashWithdrawal: {Math.Round(CashWithdrawalInEuro, 2, MidpointRounding.ToEven)} EUR, CustodyFee: ${Math.Round(CustodyFeeInEuro, 2, MidpointRounding.ToEven)} EUR";
    }
}