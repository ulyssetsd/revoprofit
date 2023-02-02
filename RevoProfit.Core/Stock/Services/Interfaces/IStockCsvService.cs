using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockCsvService
{
    Task<IEnumerable<Transaction>> ReadCsv(Stream stream);
}