using CsvHelper;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;
using System.Globalization;
using RevoProfit.Core.Extensions;

namespace RevoProfit.Core.Revolut.Services;

public class RevolutCsvService : IRevolutCsvService
{
    private readonly RevolutTransactionMapper _revolutTransaction2022Mapper = new();
    private readonly RevolutTransaction2025Mapper _revolutTransaction2025Mapper = new();

    public async Task<IEnumerable<RevolutTransaction>> ReadCsv(Stream stream)
    {
        using var peekReader = new StreamReader(stream);
        var headerLine = await peekReader.ReadLineAsync() ?? string.Empty;
        var is2025Format = headerLine.Contains("Symbol") && headerLine.Contains("Price") && headerLine.Contains("Value");
        stream.Position = 0;
        
        using var streamReader = new StreamReader(stream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        if (is2025Format)
        {
            var csvLines = await csv.GetRecordsAsync<RevolutTransaction2025CsvLine>().ToEnumerableAsync();
            return csvLines.Select(_revolutTransaction2025Mapper.Map);
        }
        else
        {
            var csvLines = await csv.GetRecordsAsync<RevolutTransactionCsvLine>().ToEnumerableAsync();
            return csvLines.Select(_revolutTransaction2022Mapper.Map);
        }
    }
}