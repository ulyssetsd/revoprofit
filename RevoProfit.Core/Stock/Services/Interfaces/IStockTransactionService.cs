using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockTransactionService
{
    (IReadOnlyCollection<StockAnnualReport> annualReports, IReadOnlyCollection<OwnedStock> stockOwneds) GetAnnualReports(IEnumerable<StockTransaction> stockTransactions);
}