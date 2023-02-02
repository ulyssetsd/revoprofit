using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockCsvService
{
    Task<IEnumerable<StockTransaction>> ReadCsv(Stream stream);
}