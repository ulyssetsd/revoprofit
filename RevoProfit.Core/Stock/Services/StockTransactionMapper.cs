using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Globalization;

namespace RevoProfit.Core.Stock.Services;

public class StockTransactionMapper : IStockTransactionMapper
{
    public Transaction Map(TransactionCsvLine source)
    {
        return new Transaction
        {
            Date = ToDateTime(source.Date),
            Ticker = source.Ticker,
            Type = ToTransactionType(source.Type),
            Quantity = ToDouble(source.Quantity),
            PricePerShare = ToDouble(source.PricePerShare),
            TotalAmount = ToDouble(source.TotalAmount),
            Currency = Currency.Usd,
            FxRate = ToDouble(source.FxRate),
        };
    }

    private static DateTime ToDateTime(string source)
    {
        if (DateTime.TryParseExact(source, "G", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var gDate))
        {
            return gDate;
        }

        if (DateTime.TryParseExact(source.Replace("Z", "0Z"), "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var oDate))
        {
            return oDate;
        }

        throw new ArgumentOutOfRangeException(source);
    }

    private static double ToDouble(string source)
    {
        if (string.IsNullOrEmpty(source)) return 0;
        return double.Parse(source, NumberStyles.Currency | NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"));
    }

    private static TransactionType ToTransactionType(string source) => source switch
    {
        "CASH TOP-UP" => TransactionType.CashTopUp,
        "BUY" or "BUY - MARKET" => TransactionType.Buy,
        "CUSTODY_FEE" or "CUSTODY FEE" => TransactionType.CustodyFee,
        "DIVIDEND" => TransactionType.Dividend,
        "SELL" or "SELL - MARKET" => TransactionType.Sell,
        "STOCK SPLIT" => TransactionType.StockSplit,
        _ => throw new ArgumentOutOfRangeException(source)
    };
}