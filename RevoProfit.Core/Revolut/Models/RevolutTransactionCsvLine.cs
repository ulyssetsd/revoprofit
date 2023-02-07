using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Revolut.Models;

public record RevolutTransactionCsvLine
{
    [Name("Type")] public string Type { get; init; }
    [Name("Product")] public string Product { get; init; }
    [Name("Started Date")] public string StartedDate { get; init; }
    [Name("Completed Date")] public string CompletedDate { get; init; }
    [Name("Description")] public string Description { get; init; }
    [Name("Amount")] public string Amount { get; init; }
    [Name("Currency")] public string Currency { get; init; }
    [Name("Fiat amount")] public string FiatAmount { get; init; }
    [Name("Fiat amount (inc. fees)")] public string FiatAmountIncludingFees { get; init; }
    [Name("Fee")] public string Fee { get; init; }
    [Name("Base currency")] public string BaseCurrency { get; init; }
    [Name("State")] public string State { get; init; }
    [Name("Balance")] public string Balance { get; init; }
}