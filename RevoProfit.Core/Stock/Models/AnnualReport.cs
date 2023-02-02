namespace RevoProfit.Core.Stock.Models;

public class AnnualReport
{
    public int Year { get; init; }
    public double Gains { get; init; }
    public double Dividends { get; init; }
    public double CashTopUp { get; init; }
    public double CashWithdrawal { get; init; }
    public double CustodyFee { get; init; }

    public double GainsInEuro { get; init; }
    public double DividendsInEuro { get; init; }
    public double CashTopUpInEuro { get; init; }
    public double CashWithdrawalInEuro { get; init; }
    public double CustodyFeeInEuro { get; init; }

    public List<SellOrder> SellOrders { get; init; }

    public override string ToString()
    {
        return $"{Year}, Gains Realized: ${Math.Round(Gains, 2, MidpointRounding.ToEven)}, Dividends: ${Math.Round(Dividends, 2, MidpointRounding.ToEven)}, CashTopUp: ${Math.Round(CashTopUp, 2, MidpointRounding.ToEven)}, CashWithdrawal: ${Math.Round(CashWithdrawal, 2, MidpointRounding.ToEven)}, CustodyFee: ${Math.Round(CustodyFee, 2, MidpointRounding.ToEven)}\n" +
               $"{Year}, Gains Realized: {Math.Round(GainsInEuro, 2, MidpointRounding.ToEven)} EUR, Dividends: {Math.Round(DividendsInEuro, 2, MidpointRounding.ToEven)} EUR, CashTopUp: {Math.Round(CashTopUpInEuro, 2, MidpointRounding.ToEven)} EUR, CashWithdrawal: {Math.Round(CashWithdrawalInEuro, 2, MidpointRounding.ToEven)} EUR, CustodyFee: ${Math.Round(CustodyFeeInEuro, 2, MidpointRounding.ToEven)} EUR";
    }
}