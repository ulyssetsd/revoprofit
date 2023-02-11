using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Globalization;

namespace RevoProfit.Core.Stock.Services;

public class StockTransactionMapper : IStockTransactionMapper
{
    public StockTransaction Map(TransactionCsvLine source)
    {
        try
        {
            return new StockTransaction
            {
                Date = ToDateTime(source.Date),
                Ticker = source.Ticker,
                Type = ToTransactionType(source.Type),
                Quantity = ToDecimal(source.Quantity),
                PricePerShare = ToDecimal(source.PricePerShare),
                TotalAmount = ToDecimal(source.TotalAmount),
                FxRate = ToDecimal(source.FxRate),
            };
        }
        catch (ProcessException exception)
        {
            throw new ProcessException($"fail to map the following line due to a {exception.Message}: {source}");
        }
    }

    private static DateTime ToDateTime(string source)
    {
        if (DateTime.TryParseExact(source, "G", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var gDate)) return gDate;
        if (DateTime.TryParseExact(source.Replace("Z", "0Z"), "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var oDate)) return oDate;
        throw new ProcessException($"fail to parse date {source}");
    }

    private static decimal ToDecimal(string source)
    {
        if (source == string.Empty) return 0;
        if (decimal.TryParse(source, NumberStyles.Currency | NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"), out var result)) return result;
        throw new ProcessException($"fail to parse decimal: {source}");
    }

    private static TransactionType ToTransactionType(string source) => source.Split(" - ").First() switch
    {
        "CASH TOP-UP" => TransactionType.CashTopUp,
        "BUY" => TransactionType.Buy, // "BUY - MARKET" and "BUY - STOP"
        "CUSTODY_FEE" or "CUSTODY FEE" => TransactionType.CustodyFee,
        "DIVIDEND" => TransactionType.Dividend,
        "SELL" => TransactionType.Sell, // "SELL - MARKET" and "SELL - STOP"
        "STOCK SPLIT" => TransactionType.StockSplit,
        "CASH WITHDRAWAL" => TransactionType.CashWithdrawal,
        _ => throw new ProcessException($"fail to parse TransactionType: {source}"),
    };
}