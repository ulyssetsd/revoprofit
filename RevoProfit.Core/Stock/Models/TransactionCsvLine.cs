using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Stock.Models;

public class TransactionCsvLine
{
    [Name("Date")] public string Date { get; set; }
    [Name("Ticker")] public string Ticker { get; set; }
    [Name("Type")] public string Type { get; set; }
    [Name("Quantity")] public string Quantity { get; set; }
    [Name("Price per share")] public string PricePerShare { get; set; }
    [Name("Total Amount")] public string TotalAmount { get; set; }
    [Name("Currency")] public string Currency { get; set; }
    [Name("FX Rate")] public string FxRate { get; set; }
}