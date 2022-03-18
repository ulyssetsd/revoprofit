using TaxRevolut.Core.Models;

namespace TaxRevolut.Core.Services.Interfaces;

public interface ITransactionService
{
    IEnumerable<AnnualReport> GetAnnualReports(IEnumerable<Transaction> transactions);
}