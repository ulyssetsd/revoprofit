using RevoProfit.Core.Services;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Globalization;

namespace RevoProfit.Core.Stock.Services;

public class StockCsvService : CsvGenericService<Transaction, TransactionCsvLine>, IStockCsvService
{
    public StockCsvService() : base(CultureInfo.GetCultureInfo("en-GB")) { }
}
