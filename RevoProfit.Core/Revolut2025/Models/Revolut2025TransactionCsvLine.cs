using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Revolut2025.Models;

public record Revolut2025TransactionCsvLine
{
    [Name("Date")] public required string Date { get; init; }
    [Name("Type")] public required string Type { get; init; }
    [Name("Symbol")] public required string Symbol { get; init; }
    [Name("Quantity")] public required string Quantity { get; init; }
    [Name("Price")] public required string Price { get; init; }
    [Name("Value")] public required string Value { get; init; }
    [Name("Fees")] public required string Fees { get; init; }
}
