namespace TaxRevolut.Core.Models;

public class AnnualGainsReport
{
    public int Year { get; set; }
    public double Gains { get; set; }
    public double Dividends { get; set; }
    public double CashTopUp { get; set; }
    public double CustodyFee { get; set; }

    public double GainsInEuro { get; set; }
    public double DividendsInEuro { get; set; }
    public double CashTopUpInEuro { get; set; }
    public double CustodyFeeInEuro { get; set; }

    public override string ToString()
    {
        return $"{Year}, Gains Realized: ${Math.Round(Gains, 2, MidpointRounding.ToEven)}, Dividends: ${Math.Round(Dividends, 2, MidpointRounding.ToEven)}, CashTopUp: ${Math.Round(CashTopUp, 2, MidpointRounding.ToEven)}, CustodyFee: ${Math.Round(CustodyFee, 2, MidpointRounding.ToEven)}\n" + 
               $"{Year}, Gains Realized: {Math.Round(GainsInEuro, 2, MidpointRounding.ToEven)} EUR, Dividends: {Math.Round(DividendsInEuro, 2, MidpointRounding.ToEven)} EUR, CashTopUp: {Math.Round(CashTopUpInEuro, 2, MidpointRounding.ToEven)} EUR, CustodyFee: ${Math.Round(CustodyFeeInEuro, 2, MidpointRounding.ToEven)} EUR";
    }
}