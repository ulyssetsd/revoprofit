using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;

namespace RevoProfit.Core.Stock.Services;

public class StockTransactionService : IStockTransactionService
{
    private const int StockDecimalsPrecision = 14;
    private const int EuroDecimalsPrecision = 14;

    public (IEnumerable<AnnualReport> annualReports, IEnumerable<StockOwned> stockOwneds) GetAnnualReports(IEnumerable<StockTransaction> stockTransactions)
    {
        var stocks = new List<StockOwned>();
        var sellOrders = new List<SellOrder>();
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
                case TransactionType.Buy:
                    {
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        stock.ValueInserted += stockTransaction.Quantity * stockTransaction.PricePerShare;
                        stock.Quantity += stockTransaction.Quantity;
                        stock.AveragePrice = stock.ValueInserted / stock.Quantity;
                        break;
                    }
                case TransactionType.Sell:
                    {
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        var equityValue = stock.Quantity * stockTransaction.PricePerShare;
                        var insertedRatio = stock.ValueInserted / equityValue;
                        var gainsRatio = 1 - insertedRatio;

                        sellOrders.Add(new SellOrder
                        {
                            Date = stockTransaction.Date,
                            Ticker = stockTransaction.Ticker,
                            Amount = stockTransaction.TotalAmount,
                            Gains = stockTransaction.TotalAmount * gainsRatio,
                            FxRate = stockTransaction.FxRate,
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
                case TransactionType.CashTopUp:
                    cashTopUps.Add(stockTransaction);
                    break;
                case TransactionType.CashWithdrawal:
                    cashWithdrawals.Add(stockTransaction);
                    break;
                case TransactionType.CustodyFee:
                    custodyFees.Add(stockTransaction);
                    break;
                case TransactionType.Dividend:
                    {
                        dividends.Add(stockTransaction);
                        var stock = GetStockOrCreate(stockTransaction.Ticker, stocks);
                        stock.TotalDividend += stockTransaction.TotalAmount;
                        break;
                    }
                case TransactionType.StockSplit:
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
                    throw new ProcessException($"TransactionType was incorrect: {stockTransaction.Type}");
            }
        }

        var annualReports = years
            .Distinct()
            .Select(year => new AnnualReport
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

                SellOrders = sellOrders.Where(order => order.Date.Year == year).ToList(),
                Gains = Math.Round(sellOrders.Where(order => order.Date.Year == year).Sum(order => order.Gains), EuroDecimalsPrecision, MidpointRounding.ToEven),
                GainsInEuro = Math.Round(sellOrders.Where(order => order.Date.Year == year).Sum(order => ConvertUsingFxRate(order.Gains, order.FxRate)), EuroDecimalsPrecision, MidpointRounding.ToEven),
            });

        return (annualReports, stocks);
    }

    private static StockOwned GetStockOrCreate(string ticker, ICollection<StockOwned> stocks)
    {
        var stock = stocks.FirstOrDefault(s => s.Ticker == ticker);
        if (stock == null)
        {
            stock = new StockOwned { Ticker = ticker };
            stocks.Add(stock);
        }

        return stock;
    }

    private static decimal ConvertUsingFxRate(decimal value, decimal fxRate)
    {
        return value * (1 / fxRate);
    }
}