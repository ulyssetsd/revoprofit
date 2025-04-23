using CsvHelper;
using RevoProfit.Core.Extensions;
using RevoProfit.Core.Revolut2025.Models;
using RevoProfit.Core.Revolut2025.Services.Interfaces;
using System.Globalization;

namespace RevoProfit.Core.Revolut2025.Services;

public class Revolut2025CsvService : IRevolut2025CsvService
{
    private readonly IRevolut2025TransactionMapper _revolutTransactionMapper;

    public Revolut2025CsvService(IRevolut2025TransactionMapper revolutTransactionMapper)
    {
        _revolutTransactionMapper = revolutTransactionMapper;
    }

    public async Task<IEnumerable<Revolut2025Transaction>> ReadCsv(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var csvLines = await csv.GetRecordsAsync<Revolut2025TransactionCsvLine>().ToEnumerableAsync();
        return csvLines.Select(_revolutTransactionMapper.Map);
    }
}