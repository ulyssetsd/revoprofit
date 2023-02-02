using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockTransactionService
{
    IEnumerable<AnnualReport> GetAnnualReports(IEnumerable<StockTransaction> transactions);
}