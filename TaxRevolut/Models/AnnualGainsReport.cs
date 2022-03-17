namespace TaxRevolut.Models;

public class AnnualGainsReport
{
    public int Year { get; set; }
    public double Gains { get; set; }
    public double Dividends { get; set; }
    public double CashTopUp { get; set; }
    public double CustodyFee { get; set; }

    public override string ToString()
    {
        return $"{Year}, Gains Realized: ${Math.Round(Gains, 2, MidpointRounding.ToEven)}, Dividends: ${Math.Round(Dividends, 2, MidpointRounding.ToEven)}, CashTopUp: ${Math.Round(CashTopUp, 2, MidpointRounding.ToEven)}, CustodyFee: ${Math.Round(CustodyFee, 2, MidpointRounding.ToEven)}";
    }
}