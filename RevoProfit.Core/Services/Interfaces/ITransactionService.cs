using RevoProfit.Core.Models;

namespace RevoProfit.Core.Services.Interfaces;

public interface ITransactionService
{
    IEnumerable<AnnualReport> GetAnnualReports(IEnumerable<Transaction> transactions);
}