using CsvHelper;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;
using System.Globalization;
using RevoProfit.Core.Extensions;

namespace RevoProfit.Core.Revolut.Services;

public class RevolutCsvService : IRevolutCsvService
{
    private readonly IRevolutTransactionMapper _revolutTransactionMapper;

    public RevolutCsvService(IRevolutTransactionMapper revolutTransactionMapper)
    {
        _revolutTransactionMapper = revolutTransactionMapper;
    }

    public async Task<IEnumerable<RevolutTransaction>> ReadCsv(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var csvLines = await csv.GetRecordsAsync<RevolutTransactionCsvLine>().ToEnumerableAsync();
        return csvLines.Select(_revolutTransactionMapper.Map);
    }
}