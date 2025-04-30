using System.Diagnostics;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;

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

    public decimal PriceInEur(ICurrencyService currencyService)
    {
        if (Price == null) throw new ProcessException("Price is null");
        if (PriceCurrency == null) throw new ProcessException("PriceCurrency is null");

        return currencyService.ConvertToEur(Price.Value, PriceCurrency.ToCurrency(), DateOnly.FromDateTime(Date));
    }

    public decimal ValueInEur(ICurrencyService currencyService)
    {
        if (Value == null) throw new ProcessException("Value is null");
        if (ValueCurrency == null) throw new ProcessException("ValueCurrency is null");        

        return currencyService.ConvertToEur(Value.Value, ValueCurrency.ToCurrency(), DateOnly.FromDateTime(Date));
    }

    public decimal FeesInEur(ICurrencyService currencyService)
    {
        if (Fees == null) throw new ProcessException("Fees is null");
        if (FeesCurrency == null) throw new ProcessException("FeesCurrency is null");

        return currencyService.ConvertToEur(Fees.Value, FeesCurrency.ToCurrency(), DateOnly.FromDateTime(Date));
    }
}
