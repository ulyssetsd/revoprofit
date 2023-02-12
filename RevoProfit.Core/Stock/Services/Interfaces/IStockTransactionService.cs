using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockTransactionService
{
    (IEnumerable<StockAnnualReport> annualReports, IEnumerable<StockOwned> stockOwneds) GetAnnualReports(IEnumerable<StockTransaction> stockTransactions);
}