using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Revolut.Models;

public record RevolutTransaction2025CsvLine
{
    [Name("Symbol")] public required string Symbol { get; init; }
    [Name("Type")] public required string Type { get; init; }
    [Name("Quantity")] public required string Quantity { get; init; }
    [Name("Price")] public required string Price { get; init; }
    [Name("Value")] public required string Value { get; init; }
    [Name("Fees")] public required string Fees { get; init; }
    [Name("Date")] public required string Date { get; init; }
}