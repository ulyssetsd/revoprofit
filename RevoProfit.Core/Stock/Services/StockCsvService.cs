using RevoProfit.Core.Services;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;

namespace RevoProfit.Core.Stock.Services;

public class StockCsvService : CsvGenericService<Transaction, TransactionCsvLine>, IStockCsvService
{
    private readonly IStockTransactionMapper _stockTransactionMapper;

    public StockCsvService(IStockTransactionMapper stockTransactionMapper)
    {
        _stockTransactionMapper = stockTransactionMapper;
    }

    public override Transaction MapCsvLineToModel(TransactionCsvLine source)
    {
        return _stockTransactionMapper.Map(source);
    }
}
