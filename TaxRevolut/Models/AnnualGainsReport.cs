namespace TaxRevolut.Models;

public class AnnualGainsReport
{
    public int Year { get; set; }
    public double TotalGains { get; set; }
    public override string ToString()
    {
        return $"{Year}, ${Math.Round(TotalGains, 2, MidpointRounding.ToEven)}";
    }
}