using System.Globalization;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;

namespace RevoProfit.Core.Revolut.Services;

public class RevolutTransactionMapper : IRevolutTransactionMapper
{
    public RevolutTransaction Map(RevolutTransactionCsvLine source)
    {
        try
        {
            return new RevolutTransaction
            {
                Description = source.Description,
                CompletedDate = ToDateTime(source.CompletedDate),
                Amount = ToDecimal(source.Amount),
                Currency = source.Currency,
                FiatAmount = ToDecimalNullable(source.FiatAmount),
                FiatAmountIncludingFees = ToDecimalNullable(source.FiatAmountIncludingFees),
                Fee = ToDecimal(source.Fee),
                BaseCurrency = source.BaseCurrency,
            };
        }
        catch (ProcessException exception)
        {
            throw new ProcessException($"fail to map the following line due to a {exception.Message}: {source}");
        }
    }

    private static DateTime ToDateTime(string source)
    {
        if(DateTime.TryParseExact(source, "yyyy-MM-dd H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) return result;
        throw new ProcessException($"fail to parse date {source}");
    }

    private static decimal ToDecimalNullable(string source)
    {
        if (source == string.Empty) return 0;
        return ToDecimal(source);
    }

    private static decimal ToDecimal(string source)
    {
        if (decimal.TryParse(source, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out var result)) return result;
        throw new ProcessException($"fail to parse decimal {source}");
    }
}