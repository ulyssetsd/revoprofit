using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface ITransactionService
{
    IEnumerable<AnnualReport> GetAnnualReports(IEnumerable<Transaction> transactions);
}