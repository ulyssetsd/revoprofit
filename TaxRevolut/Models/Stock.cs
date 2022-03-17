namespace TaxRevolut.Models;

public class Stock
{
    public string Ticker { get; set; }
    public double Quantity { get; set; }
    public double AveragePrice { get; set; }
    public double ValueInserted { get; set; }
    public double TotalDividend { get; set; }
    public override string ToString()
    {
        return $"{Ticker}, Quantity: {Math.Round(Quantity, 14, MidpointRounding.ToEven)}, AveragePrice: ${Math.Round(AveragePrice, 2, MidpointRounding.ToEven)}, ValueInserted: ${Math.Round(ValueInserted, 2, MidpointRounding.ToEven)}, TotalDividend: {TotalDividend}";
    }
}