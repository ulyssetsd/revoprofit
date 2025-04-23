using System.Globalization;
using CsvHelper.Configuration.Attributes;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Core.Revolut2025.Services;

public record RevolutTransaction2025CsvLine
{
    [Name("Date")] public required string Date { get; init; }
    [Name("Type")] public required string Type { get; init; }
    [Name("Symbol")] public required string Symbol { get; init; }
    [Name("Quantity")] public required string Quantity { get; init; }
    [Name("Price")] public required string Price { get; init; }
    [Name("Value")] public required string Value { get; init; }
    [Name("Fees")] public required string Fees { get; init; }
}

public enum RevolutTransactionType { Buy, Sell, Send, Payment, Receive, Stake, Unstake, LearnReward, StakingReward, Other }

public record RevolutTransaction2025
{
    public required DateTime Date { get; init; }
    public required RevolutTransactionType Type { get; init; }
    public required string Symbol { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal? Price { get; init; }
    public required string? PriceCurrency { get; init; }
    public required decimal? Value { get; init; }
    public required decimal? Fees { get; init; }
}

public interface IRevolutTransaction2025Mapper
{
    RevolutTransaction2025 Map(RevolutTransaction2025CsvLine source);
}

public class RevolutTransaction2025Mapper : IRevolutTransaction2025Mapper
{
    public RevolutTransaction2025 Map(RevolutTransaction2025CsvLine source)
    {
        try
        {
            return new RevolutTransaction2025
            {
                Date = ToDateTime(source.Date),
                Type = source.Type switch
                {
                    "Buy" => RevolutTransactionType.Buy,
                    "Sell" => RevolutTransactionType.Sell,
                    "Send" => RevolutTransactionType.Send,
                    "Payment" => RevolutTransactionType.Payment,
                    "Receive" => RevolutTransactionType.Receive,
                    "Stake" => RevolutTransactionType.Stake,
                    "Unstake" => RevolutTransactionType.Unstake,
                    "Learn reward" => RevolutTransactionType.LearnReward,
                    "Staking reward" => RevolutTransactionType.StakingReward,
                    "Other" => RevolutTransactionType.Other,
                    _ => throw new ProcessException($"Unknown transaction type: {source.Type}")
                },
                Symbol = source.Symbol,
                Quantity = ToDecimal(source.Quantity),
                Price = ToNullableDecimalWithCurrency(source.Price),
                PriceCurrency = GetBaseCurrency(source.Price),
                Value = ToNullableDecimalWithCurrency(source.Value),
                Fees = ToNullableDecimalWithCurrency(source.Fees),
            };
        }
        catch (ProcessException exception)
        {
            throw new ProcessException($"fail to map the following line due to a {exception.Message}: {source}");
        }
    }

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