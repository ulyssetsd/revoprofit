namespace TaxRevolut.Models;

public class Transaction
{
    public DateTime Date { get; set; }
    public string Ticker { get; set; }
    public TransactionType Type { get; set; }
    public double Quantity { get; set; }
    public double PricePerShare { get; set; }
    public double TotalAmount { get; set; }
    public Currency Currency { get; set; }
    public double FXRate { get; set; }
}