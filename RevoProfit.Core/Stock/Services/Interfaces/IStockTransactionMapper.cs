using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockTransactionMapper
{
    StockTransaction Map(StockTransactionCsvLine source);
}