using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Revolut.Models;

public record RevolutTransactionCsvLine
{
    [Name("Type")] public required string Type { get; init; }
    [Name("Product")] public required string Product { get; init; }
    [Name("Started Date")] public required string StartedDate { get; init; }
    [Name("Completed Date")] public required string CompletedDate { get; init; }
    [Name("Description")] public required string Description { get; init; }
    [Name("Amount")] public required string Amount { get; init; }
    [Name("Currency")] public required string Currency { get; init; }
    [Name("Fiat amount")] public required string FiatAmount { get; init; }
    [Name("Fiat amount (inc. fees)")] public required string FiatAmountIncludingFees { get; init; }
    [Name("Fee")] public required string Fee { get; init; }
    [Name("Base currency")] public required string BaseCurrency { get; init; }
    [Name("State")] public required string State { get; init; }
    [Name("Balance")] public required string Balance { get; init; }
}