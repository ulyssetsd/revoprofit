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

public enum RevolutTransactionType { Buy, Sell, Send, Payment, Receive, Exchange, Stake, Unstake, LearnReward, StakingReward, Other }

public record RevolutTransaction2025
{
    public required DateTime Date { get; init; }
    public required RevolutTransactionType Type { get; init; }
    public required string Symbol { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public required string? PriceCurrency { get; init; }
    public required decimal Value { get; init; }
    public required decimal Fees { get; init; }
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
                    "Exchange" => RevolutTransactionType.Exchange,
                    "Stake" => RevolutTransactionType.Stake,
                    "Unstake" => RevolutTransactionType.Unstake,
                    "Learn reward" => RevolutTransactionType.LearnReward,
                    "Staking reward" => RevolutTransactionType.StakingReward,
                    _ => throw new ProcessException($"Unknown transaction type: {source.Type}")
                },
                Symbol = source.Symbol,
                Quantity = ToDecimal(source.Quantity),
                Price = ToDecimalWithCurrency(source.Price),
                PriceCurrency = GetBaseCurrency(source.Price),
                Value = ToDecimalWithCurrency(source.Value),
                Fees = ToDecimalWithCurrency(source.Fees),
            };
        }
        catch (ProcessException exception)
        {
            throw new ProcessException($"fail to map the following line due to a {exception.Message}: {source}");
        }
    }

    private static (string description, string type) GetDescriptionAndTransactionType(string type) => type switch
    {
        "Buy" => ("Buy crypto", type),
        "Sell" => ("Sell crypto", type),
        "Send" => ("Send crypto", type),
        "Payment" => ("Crypto payment", type),
        "Receive" => ("Receive crypto", type),
        "Exchange" => ("Exchange crypto", type),
        "Stake" => ("Stake crypto", type),
        "Unstake" => ("Unstake crypto", type),
        "Learn reward" => ("Crypto learn reward", type),
        "Staking reward" => ("Crypto staking reward", type),
        "Other" => ("Other crypto transaction", type),
        _ => throw new ProcessException($"Unknown transaction type: {type}")
    };

    private static string GetBaseCurrency(string price) => 
        price.StartsWith("€") ? "EUR" : 
        price.StartsWith("$") ? "USD" : 
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

    private static decimal ToDecimalWithCurrency(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return 0;
        
        var normalized = source.Replace("€", "").Replace("$", "").Replace(",", "").Trim();
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new ProcessException($"fail to parse decimal with currency {source}");
    }
}