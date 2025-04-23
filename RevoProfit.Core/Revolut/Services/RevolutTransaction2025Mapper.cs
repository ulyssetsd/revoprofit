using System.Globalization;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;

namespace RevoProfit.Core.Revolut.Services;

public class RevolutTransaction2025Mapper : IRevolutTransactionMapper 
{
    public RevolutTransaction Map(RevolutTransaction2025CsvLine source)
    {
        try
        {
            var (description, transactionType) = GetDescriptionAndTransactionType(source.Type);
            return new RevolutTransaction
            {
                Description = description,
                CompletedDate = ToDateTime(source.Date),
                Amount = IsNegativeAmountType(transactionType) ? -ToDecimal(source.Quantity) : ToDecimal(source.Quantity),
                Currency = source.Symbol,
                FiatAmount = string.IsNullOrWhiteSpace(source.Value) ? 0 : ToDecimalWithCurrency(source.Value),
                FiatAmountIncludingFees = string.IsNullOrWhiteSpace(source.Value) ? 0 : ToDecimalWithCurrency(source.Value) + ToDecimalWithCurrency(source.Fees ?? "0"),
                FiatFees = string.IsNullOrWhiteSpace(source.Fees) ? 0 : ToDecimalWithCurrency(source.Fees),
                BaseCurrency = string.IsNullOrWhiteSpace(source.Price) ? "EUR" : GetBaseCurrency(source.Price), // Default to EUR if no price given
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

    private static bool IsNegativeAmountType(string type) => type switch
    {
        "Sell" or "Send" or "Payment" or "Stake" => true,
        _ => false
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
        
        // Remove currency symbols and normalize
        var normalized = source.Replace("€", "").Replace("$", "").Replace(",", "").Trim();
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new ProcessException($"fail to parse decimal with currency {source}");
    }

    RevolutTransaction IRevolutTransactionMapper.Map(RevolutTransactionCsvLine source) => 
        throw new NotImplementedException("2025 mapper cannot handle 2022 format");
}