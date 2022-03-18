namespace TaxRevolut.Core.Models;

public class SellOrder
{
    public DateTime Date { get; set; }
    public string Ticker { get; set; }
    public double Amount { get; set; }
    public double Gains { get; set; }
    public override string ToString()
    {
        return $"{Date}, {Ticker}, ${Math.Round(Amount, 2, MidpointRounding.ToEven)}, ${Math.Round(Gains, 2, MidpointRounding.ToEven)}";
    }
}