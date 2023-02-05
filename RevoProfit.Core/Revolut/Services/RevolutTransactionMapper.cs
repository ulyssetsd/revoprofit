using System.Globalization;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;

namespace RevoProfit.Core.Revolut.Services;

public class RevolutTransactionMapper : IRevolutTransactionMapper
{
    public RevolutTransaction Map(RevolutTransactionCsvLine source)
    {
        return new RevolutTransaction
        {
            Description = source.Description,
            CompletedDate = ToDateTime(source.CompletedDate),
            Amount = ToDecimal(source.Amount),
            Currency = source.Currency,
            FiatAmount = ToDecimal(source.FiatAmount),
            FiatAmountIncludingFees = ToDecimal(source.FiatAmountIncludingFees),
            Fee = ToDecimal(source.Fee),
            BaseCurrency = source.BaseCurrency,
        };
    }

    private static DateTime ToDateTime(string source)
    {
        return DateTime.ParseExact(source, "yyyy-MM-dd H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    private static decimal ToDecimal(string source)
    {
        if (decimal.TryParse(source, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out var result)) return result;
        throw new InvalidOperationException(source);
    }
}