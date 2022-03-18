using System.Data;
using TaxRevolut.Core.Models;

namespace TaxRevolut.Core.Services;

public class TransactionService
{
    private List<Stock> Stocks { get; } = new();
    private List<SellOrder> SellOrders { get; } = new();
    private List<Transaction> Dividends { get; } = new();
    private List<Transaction> CashTopUps { get; } = new();
    private List<Transaction> CustodyFees { get; } = new();
    private List<int> Years { get; } = new();

    public void ProcessTransactions(IEnumerable<Transaction> transactions)
    {
        foreach (var transaction in transactions.OrderBy(transaction => transaction.Date))
        {
            AddTransaction(transaction);
        }
    }

    private void AddTransaction(Transaction transaction)
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
        else if (transaction.Type == TransactionType.Unknown)
        {
            throw new DataException();
        }
        else
        {
            throw new DataException();
        }
    }

    private Stock GetStockOrCreate(string ticker)
    {
        var stock = Stocks.FirstOrDefault(s => s.Ticker == ticker);
        if (stock == null)
        {
            stock = new Stock { Ticker = ticker };
            Stocks.Add(stock);
        }

        return stock;
    }

    public IEnumerable<Stock> GetCurrentStocks()
    {
        return Stocks.OrderBy(stock => stock.Ticker)
            .Where(stock => Math.Round(stock.Quantity, 14, MidpointRounding.ToEven) != 0);
    }

    public IEnumerable<Stock> GetOldStocks()
    {
        return Stocks.OrderBy(stock => stock.Ticker)
            .Where(stock => Math.Round(stock.Quantity, 14, MidpointRounding.ToEven) == 0);
    }

    public IEnumerable<SellOrder> GetSellOrders()
    {
        return SellOrders;
    }

    public IEnumerable<AnnualGainsReport> GetAnnualGainsReports()
    {
        return Years
            .Distinct()
            .Select(year =>
            {
                var totalGains = SellOrders
                    .Where(order => order.Date.Year == year)
                    .Sum(order => order.Gains);
                var totalDividends = Dividends
                    .Where(transaction => transaction.Date.Year == year)
                    .Sum(transaction => transaction.TotalAmount);
                var totalCashTopUp = CashTopUps
                    .Where(transaction => transaction.Date.Year == year)
                    .Sum(transaction => transaction.TotalAmount);
                var totalCustodyFee = CustodyFees
                    .Where(transaction => transaction.Date.Year == year)
                    .Sum(transaction => transaction.TotalAmount);
                return new AnnualGainsReport
                {
                    Year = year,
                    Gains = totalGains,
                    Dividends = totalDividends,
                    CashTopUp = totalCashTopUp,
                    CustodyFee = totalCustodyFee,
                };
            });
    }
}