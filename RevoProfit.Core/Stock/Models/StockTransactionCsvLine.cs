using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Stock.Models;

public record StockTransactionCsvLine
{
    [Name("Date")] public required string Date { get; init; }
    [Name("Ticker")] public required string Ticker { get; init; }
    [Name("Type")] public required string Type { get; init; }
    [Name("Quantity")] public required string Quantity { get; init; }
    [Name("Price per share")] public required string PricePerShare { get; init; }
    [Name("Total Amount")] public required string TotalAmount { get; init; }
    [Name("Currency")] public required string Currency { get; init; }
    [Name("FX Rate")] public required string FxRate { get; init; }
}