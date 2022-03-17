using System.Data;
using TaxRevolut.Models;

namespace TaxRevolut.Services;

public class TransactionService
{
    private List<Stock> Stocks { get; } = new();
    private List<SellOrder> SellOrders { get; } = new();

    public void AddTransaction(Transaction transaction)
    {
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
            stock.ValueInserted -= transaction.TotalAmount * (1 - insertedRatio);
        }
        else if (transaction.Type == TransactionType.CashTopUp)
        {
            //TODO
        }
        else if (transaction.Type == TransactionType.CustodyFee)
        {
            var stock = GetStockOrCreate(transaction.Ticker);
            stock.TotalDividend += transaction.TotalAmount;
        }
        else if (transaction.Type == TransactionType.Dividend)
        {
            //TODO
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

    public IEnumerable<SellOrder> GetSellOrders()
    {
        return SellOrders;
    }

    public IEnumerable<AnnualGainsReport> GetAnnualGainsReports()
    {
        return SellOrders
            .GroupBy(order => order.Date.Year)
            .Select(orders =>
            {
                var year = orders.Key;
                var totalGains = orders.Sum(order => order.Gains);
                return new AnnualGainsReport
                {
                    Year = year,
                    TotalGains = totalGains,
                };
            });
    }
}