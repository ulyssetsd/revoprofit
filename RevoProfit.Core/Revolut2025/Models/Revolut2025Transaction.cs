namespace RevoProfit.Core.Revolut2025.Models;

public record Revolut2025Transaction
{
    public required DateTime Date { get; init; }
    public required Revolut2025TransactionType Type { get; init; }
    public required string Symbol { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal? Price { get; init; }
    public required string? PriceCurrency { get; init; }
    public required decimal? Value { get; init; }
    public string? ValueCurrency { get; init; }
    public required decimal? Fees { get; init; }
    public string? FeesCurrency { get; init; }
}
