using CsvHelper;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Globalization;
using RevoProfit.Core.Extensions;

namespace RevoProfit.Core.Stock.Services;

public class StockCsvService : IStockCsvService
{
    private readonly IStockTransactionMapper _stockTransactionMapper;

    public StockCsvService(IStockTransactionMapper stockTransactionMapper)
    {
        _stockTransactionMapper = stockTransactionMapper;
    }

    public async Task<IEnumerable<StockTransaction>> ReadCsv(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var csvLines = await csv.GetRecordsAsync<TransactionCsvLine>().ToEnumerableAsync();
        return csvLines.Select(_stockTransactionMapper.Map);
    }
}
