using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;

namespace RevoProfit.Core.Stock.Services;

public class StockTransactionService : IStockTransactionService
{
    private const int StockDecimalsPrecision = 14;
    private const int EuroDecimalsPrecision = 14;

    public (IReadOnlyCollection<StockAnnualReport> annualReports, IReadOnlyCollection<OwnedStock> stockOwneds) GetAnnualReports(IEnumerable<StockTransaction> stockTransactions)
    {
        var stocks = new List<OwnedStock>();
        var stockSellOrders = new List<StockSellOrder>();
        var dividends = new List<StockTransaction>();
        var cashTopUps = new List<StockTransaction>();
        var cashWithdrawals = new List<StockTransaction>();
        var custodyFees = new List<StockTransaction>();
        var years = new List<int>();

        foreach (var stockTransaction in stockTransactions.OrderBy(transaction => transaction.Date))
        {
            years.Add(stockTransaction.Date.Year);
            switch (stockTransaction.Type)
            {
                case StockTransactionType.Buy:
                    {
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        stock.ValueInserted += stockTransaction.Quantity * stockTransaction.PricePerShare;
                        stock.Quantity += stockTransaction.Quantity;
                        stock.AveragePrice = stock.ValueInserted / stock.Quantity;
                        break;
                    }
                case StockTransactionType.Sell:
                    {
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        var equityValue = stock.Quantity * stockTransaction.PricePerShare;
                        var insertedRatio = stock.ValueInserted / equityValue;
                        var gainsRatio = 1 - insertedRatio;

                        var gains = Math.Round(stockTransaction.TotalAmount * gainsRatio, EuroDecimalsPrecision, MidpointRounding.ToEven);
                        stockSellOrders.Add(new StockSellOrder
                        {
                            Date = stockTransaction.Date,
                            Ticker = stockTransaction.Ticker,
                            Amount = stockTransaction.TotalAmount,
                            Gains = gains,
                            GainsInEuros = ConvertUsingFxRate(gains, stockTransaction.FxRate),
                            Quantity = stockTransaction.Quantity,
                        });

                        stock.Quantity -= stockTransaction.Quantity;
                        stock.ValueInserted -= Math.Round(stockTransaction.TotalAmount * insertedRatio, EuroDecimalsPrecision, MidpointRounding.ToEven);

                        if (Math.Round(stock.Quantity, StockDecimalsPrecision, MidpointRounding.ToEven) == 0)
                        {
                            stock.AveragePrice = 0;
                            stock.ValueInserted = 0;
                        }
                        break;
                    }
                case StockTransactionType.CashTopUp:
                    cashTopUps.Add(stockTransaction);
                    break;
                case StockTransactionType.CashWithdrawal:
                    cashWithdrawals.Add(stockTransaction);
                    break;
                case StockTransactionType.CustodyFee:
                    custodyFees.Add(stockTransaction);
                    break;
                case StockTransactionType.CustodyFeeReversal:
                    custodyFees.Add(stockTransaction with { TotalAmount = -stockTransaction.TotalAmount });
                    break;
                case StockTransactionType.Dividend:
                    {
                        dividends.Add(stockTransaction);
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        stock.TotalDividend += stockTransaction.TotalAmount;
                        break;
                    }
                case StockTransactionType.StockSplit:
                    {
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        var previousQuantity = stock.Quantity;
                        var newQuantity = stock.Quantity + stockTransaction.Quantity;
                        var ratio = previousQuantity / newQuantity;
                        stock.Quantity = newQuantity;
                        stock.AveragePrice *= ratio;
                        break;
                    }
                default:
                    throw new ProcessException($"StockTransactionType was incorrect: {stockTransaction.Type}");
            }
        }

        var annualReports = years
            .Distinct()
            .Select(year => new StockAnnualReport
            {
                Year = year,

                Dividends = dividends.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                CashTopUp = cashTopUps.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                CashWithdrawal = cashWithdrawals.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                CustodyFee = custodyFees.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),

                DividendsInEuro = dividends.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                CashTopUpInEuro = cashTopUps.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                CashWithdrawalInEuro = cashWithdrawals.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                CustodyFeeInEuro = custodyFees.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),

                SellReport = new StockSellAnnualReport
                {
                    StockSellOrders = stockSellOrders.Where(order => order.Date.Year == year).ToList(),
                    Gains = Math.Round(stockSellOrders.Where(order => order.Date.Year == year).Sum(order => order.Gains), EuroDecimalsPrecision, MidpointRounding.ToEven),
                    GainsInEuro = Math.Round(stockSellOrders.Where(order => order.Date.Year == year).Sum(order => order.GainsInEuros), EuroDecimalsPrecision, MidpointRounding.ToEven),
                },
            })
            .ToList();

        return (annualReports, stocks);
    }

    private static OwnedStock GetStockOrCreate(string ticker, ICollection<OwnedStock> stocks)
    {
        var stock = stocks.FirstOrDefault(s => s.Ticker == ticker);
        if (stock == null)
        {
            stock = new OwnedStock { Ticker = ticker };
            stocks.Add(stock);
        }

        return stock;
    }

    private static decimal ConvertUsingFxRate(decimal value, decimal fxRate)
    {
        return value * (1 / fxRate);
    }
}