using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Data;

namespace RevoProfit.Core.Stock.Services;

public class StockTransactionService : IStockTransactionService
{
    private const int StockDecimalsPrecision = 14;
    private const int EuroDecimalsPrecision = 14;
    private List<StockOwned> Stocks { get; } = new();
    private List<SellOrder> SellOrders { get; } = new();
    private List<StockTransaction> Dividends { get; } = new();
    private List<StockTransaction> CashTopUps { get; } = new();
    private List<StockTransaction> CashWithdrawals { get; } = new();
    private List<StockTransaction> CustodyFees { get; } = new();
    private List<int> Years { get; } = new();

    public IEnumerable<AnnualReport> GetAnnualReports(IEnumerable<StockTransaction> transactions)
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
        CashWithdrawals.Clear();
        CustodyFees.Clear();
        Years.Clear();
    }

    public void ProcessTransactions(IEnumerable<StockTransaction> transactions)
    {
        foreach (var transaction in transactions.OrderBy(transaction => transaction.Date))
        {
            ProcessTransaction(transaction);
        }
    }

    private void ProcessTransaction(StockTransaction stockTransaction)
    {
        Years.Add(stockTransaction.Date.Year);
        switch (stockTransaction.Type)
        {
            case TransactionType.Buy:
            {
                var stock = GetStockOrCreate(stockTransaction.Ticker);
                stock.ValueInserted += stockTransaction.Quantity * stockTransaction.PricePerShare;
                stock.Quantity += stockTransaction.Quantity;
                stock.AveragePrice = stock.ValueInserted / stock.Quantity;
                break;
            }
            case TransactionType.Sell:
            {
                var stock = GetStockOrCreate(stockTransaction.Ticker);
                var equityValue = stock.Quantity * stockTransaction.PricePerShare;
                var insertedRatio = stock.ValueInserted / equityValue;
                var gainsRatio = 1 - insertedRatio;

                SellOrders.Add(new SellOrder
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
                CashTopUps.Add(stockTransaction);
                break;
            case TransactionType.CashWithdrawal:
                CashWithdrawals.Add(stockTransaction);
                break;
            case TransactionType.CustodyFee:
                CustodyFees.Add(stockTransaction);
                break;
            case TransactionType.Dividend:
            {
                Dividends.Add(stockTransaction);
                var stock = GetStockOrCreate(stockTransaction.Ticker);
                stock.TotalDividend += stockTransaction.TotalAmount;
                break;
            }
            case TransactionType.StockSplit:
            {
                var stock = GetStockOrCreate(stockTransaction.Ticker);
                var previousQuantity = stock.Quantity;
                var newQuantity = stock.Quantity + stockTransaction.Quantity;
                var ratio = previousQuantity / newQuantity;
                stock.Quantity = newQuantity;
                stock.AveragePrice *= ratio;
                break;
            }
            default:
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
            .Where(stock => Math.Round(stock.Quantity, StockDecimalsPrecision, MidpointRounding.ToEven) != 0);
    }

    public IEnumerable<StockOwned> GetOldStocks()
    {
        return Stocks.OrderBy(stock => stock.Ticker)
            .Where(stock => Math.Round(stock.Quantity, StockDecimalsPrecision, MidpointRounding.ToEven) == 0);
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

                    Gains = Math.Round(SellOrders.Where(order => order.Date.Year == year).Sum(order => order.Gains), EuroDecimalsPrecision, MidpointRounding.ToEven),
                    Dividends = Dividends.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                    CashTopUp = CashTopUps.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                    CashWithdrawal = CashWithdrawals.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),
                    CustodyFee = CustodyFees.Where(transaction => transaction.Date.Year == year).Sum(transaction => transaction.TotalAmount),

                    GainsInEuro = SellOrders.Where(order => order.Date.Year == year).Sum(order => ConvertUsingFxRate(order.Gains, order.FxRate)),
                    DividendsInEuro = Dividends.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                    CashTopUpInEuro = CashTopUps.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                    CashWithdrawalInEuro = CashWithdrawals.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),
                    CustodyFeeInEuro = CustodyFees.Where(transaction => transaction.Date.Year == year).Sum(transaction => ConvertUsingFxRate(transaction.TotalAmount, transaction.FxRate)),

                    SellOrders = SellOrders.Where(order => order.Date.Year == year).ToList(),
                };
            });
    }

    private static decimal ConvertUsingFxRate(decimal value, decimal fxRate)
    {
        return value * (1 / fxRate);
    }
}