using System.Globalization;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut2025.Models;
using RevoProfit.Core.Revolut2025.Services.Interfaces;

namespace RevoProfit.Core.Revolut2025.Services;

public class Revolut2025TransactionMapper : IRevolut2025TransactionMapper
{
    public Revolut2025Transaction Map(Revolut2025TransactionCsvLine source)
    {
        try
        {
            return new Revolut2025Transaction
            {
                Date = ToDateTime(source.Date),
                Type = ToType(source.Type),
                Symbol = source.Symbol,
                Quantity = ToDecimal(source.Quantity),
                Price = ToNullableDecimalWithCurrency(source.Price),
                PriceCurrency = GetBaseCurrency(source.Price),
                Value = ToNullableDecimalWithCurrency(source.Value),
                ValueCurrency = GetBaseCurrency(source.Value),
                Fees = ToNullableDecimalWithCurrency(source.Fees),
                FeesCurrency = GetBaseCurrency(source.Fees),
            };
        }
        catch (ProcessException exception)
        {
            throw new ProcessException($"fail to map the following line due to a {exception.Message}: {source}");
        }
    }

    private static Revolut2025TransactionType ToType(string type) => type switch
    {
        "Buy" => Revolut2025TransactionType.Buy,
        "Sell" => Revolut2025TransactionType.Sell,
        "Send" => Revolut2025TransactionType.Send,
        "Payment" => Revolut2025TransactionType.Payment,
        "Receive" => Revolut2025TransactionType.Receive,
        "Stake" => Revolut2025TransactionType.Stake,
        "Unstake" => Revolut2025TransactionType.Unstake,
        "Learn reward" => Revolut2025TransactionType.LearnReward,
        "Staking reward" => Revolut2025TransactionType.StakingReward,
        "Other" => Revolut2025TransactionType.Other,
        _ => throw new ProcessException($"Unknown transaction type: {type}")
    };

    private static string? GetBaseCurrency(string price) =>
        string.IsNullOrWhiteSpace(price) ? null :
        price.StartsWith('€') ? "EUR" :
        price.StartsWith('$') ? "USD" :
        throw new ProcessException($"Unknown currency symbol in price: {price}");

    private static DateTime ToDateTime(string source)
    {
        if (DateTime.TryParseExact(source, "MMM d, yyyy, h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;
        throw new ProcessException($"fail to parse date {source}");
    }

    private static decimal ToDecimal(string source)
    {
        if (decimal.TryParse(source, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new ProcessException($"fail to parse decimal {source}");
    }

    private static decimal? ToNullableDecimalWithCurrency(string source)
    {
        if (source is null) return null;
        var normalized = source.Replace("€", "").Replace("$", "").Replace(",", "").Trim();
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new ProcessException($"fail to parse nullable decimal with currency {source}");
    }
}