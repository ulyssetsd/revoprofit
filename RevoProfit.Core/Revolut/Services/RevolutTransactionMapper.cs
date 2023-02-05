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
            Type = ToRevolutTransactionType(source.Type),
            Description = source.Description,
            CompletedDate = ToDateTime(source.CompletedDate),
            Amount = ToDecimal(source.Amount),
            Currency = source.Currency,
            FiatAmount = ToDecimal(source.FiatAmount),
            FiatAmountIncludingFees = ToDecimal(source.FiatAmountIncludingFees),
            Fee = ToDecimal(source.Fee),
            BaseCurrency = source.BaseCurrency,
            Balance = ToDoubleNullable(source.Balance),
        };
    }

    private static DateTime ToDateTime(string source)
    {
        return DateTime.ParseExact(source, "yyyy-MM-dd H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    private static decimal? ToDoubleNullable(string source)
    {
        if (string.IsNullOrEmpty(source)) return null;
        return ToDecimal(source);
    }

    private static decimal ToDecimal(string source)
    {
        if (decimal.TryParse(source, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out var result)) return result;
        throw new InvalidOperationException(source);
    }

    private static RevolutTransactionType ToRevolutTransactionType(string source) => source switch
    {
        "EXCHANGE" => RevolutTransactionType.Exchange,
        "TRANSFER" => RevolutTransactionType.Transfer,
        "CASHBACK" => RevolutTransactionType.CashBack,
        "CARD_PAYMENT" => RevolutTransactionType.CardPayment,
        "CARD_REFUND" => RevolutTransactionType.CardRefund,
        "REWARD" => RevolutTransactionType.Reward,
        _ => throw new ArgumentOutOfRangeException(source),
    };
}