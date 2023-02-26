namespace RevoProfit.Core.Revolut.Models;

public record RevolutTransaction
{
    public required DateTime CompletedDate { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required decimal FiatAmount { get; init; }
    public required decimal FiatAmountIncludingFees { get; init; }
    public required decimal FiatFees { get; init; }
    public required string BaseCurrency { get; init; }
}