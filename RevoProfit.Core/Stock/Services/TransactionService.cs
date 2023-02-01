using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Data;

namespace RevoProfit.Core.Stock.Services;

public class TransactionService : ITransactionService
{
    private List<StockOwned> Stocks { get; } = new();
    private List<SellOrder> SellOrders { get; } = new();
    private List<Transaction> Dividends { get; } = new();
    private List<Transaction> CashTopUps { get; } = new();
    private List<Transaction> CustodyFees { get; } = new();
    private List<int> Years { get; } = new();

    public IEnumerable<AnnualReport> GetAnnualReports(IEnumerable<Transaction> transactions)
    {
        ClearData();
        ProcessTransactions(transactions);
        return GetAnnualGainsReports();
    }

    private void ClearData()
    {
        Stocks.Clear();
        SellOrders.Clear();
        Dividends.Clear();
        CashTopUps.Clear();
        CustodyFees.Clear();
        Years.Clear();
    }

    public void ProcessTransactions(IEnumerable<Transaction> transactions)
    {
        foreach (var transaction in transactions.OrderBy(transaction => transaction.Date))
        {
            ProcessTransaction(transaction);
        }
    }

    private void ProcessTransaction(Transaction transaction)
    {
        Years.Add(transaction.Date.Year);
        if (transaction.Type == TransactionType.Buy)
        {
            var stock = GetStockOrCreate(transaction.Ticker);
            stock.ValueInserted += transaction.Quantity * transaction.PricePerShare;
            stock.Quantity += transaction.Quantity;
            stock.AveragePrice = stock.ValueInserted / stock.Quantity;
        }
        else if (transaction.Type == TransactionType.Sell)
        {
            var stock = GetStockOrCreate(transaction.Ticker);
            var equityValue = stock.Quantity * transaction.PricePerShare;
            var insertedRatio = stock.ValueInserted / equityValue;
            var gainsRatio = 1 - insertedRatio;

            SellOrders.Add(new SellOrder
            {
                Date = transaction.Date,
                Ticker = transaction.Ticker,
                Amount = transaction.TotalAmount,
                Gains = transaction.TotalAmount * gainsRatio,
                FxRate = transaction.FxRate,
            });

            stock.Quantity -= transaction.Quantity;
            stock.ValueInserted -= transaction.TotalAmount * insertedRatio;

            if (Math.Round(stock.Quantity, 14, MidpointRounding.ToEven) == 0)
            {
                stock.AveragePrice = 0;
                stock.ValueInserted = 0;
            }
        }
        else if (transaction.Type == TransactionType.CashTopUp)
        {
            CashTopUps.Add(transaction);
        }
        else if (transaction.Type == TransactionType.CustodyFee)
        {
            CustodyFees.Add(transaction);
        }
        else if (transaction.Type == TransactionType.Dividend)
        {
            Dividends.Add(transaction);
            var stock = GetStockOrCreate(transaction.Ticker);
            stock.TotalDividend += transaction.TotalAmount;
        }
        else if (transaction.Type == TransactionType.StockSplit)
        {
            var stock = GetStockOrCreate(transaction.Ticker);
            var previousQuantity = stock.Quantity;
            var newQuantity = stock.Quantity + transaction.Quantity;
            var ratio = previousQuantity / newQuantity;
            stock.Quantity = newQuantity;
            stock.AveragePrice *= ratio;
        }
        else
        {
            throw new DataException();
        }
    }

    private StockOwned GetStockOrCreate(string ticker)
    {
        var stock = Stocks.FirstOrDefault(s => s.Ticker == ticker);
        if (stock == null)
        {
            stock = new StockOwned { Ticker = ticker };
            Stocks.Add(stock);
        }

        return stock;
    }

    public IEnumerable<StockOwned> GetCurrentStocks()
    {
        return Stocks.OrderBy(stock => stock.Ticker)
            .Where(stock => Math.Round(stock.Quantity, 14, MidpointRounding.ToEven) != 0);
    }

    public IEnumerable<StockOwned> GetOldStocks()
    {
        return Stocks.OrderBy(stock => stock.Ticker)
            .Where(stock => Math.Round(stock.Quantity, 14, MidpointRounding.ToEven) == 0);
    }

    public IEnumerable<AnnualReport> GetAnnualGainsReports()
    {
        return Years
            .Distinct()
            .Select(year =>
            {
                return new AnnualReport
                {
                    Year = year,

                    Gains = SellOrders.Where(order => order.Date.Year == year).Sum(order => order.Gains),
                    Dividends = Dividends.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                    CashTopUp = CashTopUps.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                    CustodyFee = CustodyFees.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),

                    GainsInEuro = SellOrders.Where(order => order.Date.Year == year).Sum(order => ConvertUsingFxRate(order.Gains, order.FxRate)),
                    DividendsInEuro = Dividends.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                    CashTopUpInEuro = CashTopUps.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                    CustodyFeeInEuro = CustodyFees.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),

                    SellOrders = SellOrders.Where(order => order.Date.Year == year).ToList(),
                };
            });
    }

    private static double ConvertUsingFxRate(double value, double fxRate)
    {
        return value * (1 / fxRate);
    }
}