using CsvHelper.Configuration.Attributes;

namespace TaxRevolut.Models;

public class CsvLine
{
    [Name("Date")] public string Date { get; set; }
    [Name("Ticker")] public string Ticker { get; set; }
    [Name("Type")] public string Type { get; set; }
    [Name("Quantity")] public string Quantity { get; set; }
    [Name("Price per share")] public string PricePerShare { get; set; }
    [Name("Total Amount")] public string TotalAmount { get; set; }
    [Name("Currency")] public string Currency { get; set; }
    [Name("FX Rate")] public string FXRate { get; set; }
}