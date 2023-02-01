using System.Globalization;
using AutoMapper;
using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Mapping;

public static class StockMapper
{
    public static void CreateMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<TransactionCsvLine, Transaction>().ConvertUsing(source => ToTransaction(source));
    }

    private static Transaction ToTransaction(TransactionCsvLine source)
    {
        return new Transaction
        {
            Date = ToDateTime(source.Date),
            Ticker = source.Ticker,
            Type = ToTransactionType(source.Type),
            Quantity = ToDouble(source.Quantity),
            PricePerShare = CurrencyStringToDouble(source.PricePerShare),
            TotalAmount = CurrencyStringToDouble(source.TotalAmount),
            Currency = Currency.Usd,
            FxRate = ToDouble(source.FxRate),
        };
    }

    private static DateTime ToDateTime(string source)
    {
        if (DateTime.TryParseExact(source, "G", new CultureInfo("en-gb"), DateTimeStyles.None, out var gDate))
        {
            return gDate;
        }

        if (DateTime.TryParseExact(source.Replace("Z", "0Z"), "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var oDate))
        {
            return oDate;
        }

        throw new ArgumentOutOfRangeException(source);
    }

    private static double CurrencyStringToDouble(string source)
    {
        return double.TryParse(source.Replace("$", string.Empty), out var output) ? output : 0;
    }

    private static double ToDouble(string source)
    {
        return double.TryParse(source, out var output) ? output : 0;
    }

    private static TransactionType ToTransactionType(string source)
    {
        return source switch
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
}