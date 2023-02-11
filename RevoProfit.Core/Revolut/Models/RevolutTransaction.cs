﻿namespace RevoProfit.Core.Revolut.Models;

public record RevolutTransaction
{
    public DateTime CompletedDate { get; init; }
    public string Description { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public decimal FiatAmount { get; init; }
    public decimal FiatAmountIncludingFees { get; init; }
    public decimal Fee { get; init; }
    public string BaseCurrency { get; init; }
}