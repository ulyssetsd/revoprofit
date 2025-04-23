using CsvHelper;
using RevoProfit.Core.Extensions;
using System.Globalization;

namespace RevoProfit.Core.Revolut2025.Services;

public interface IRevolutCsvService2025
{
    Task<IEnumerable<RevolutTransaction2025>> ReadCsv(Stream stream);
}

public class RevolutCsvService2025 : IRevolutCsvService2025
{
    private readonly IRevolutTransaction2025Mapper _revolutTransactionMapper;

    public RevolutCsvService2025(IRevolutTransaction2025Mapper revolutTransactionMapper)
    {
        _revolutTransactionMapper = revolutTransactionMapper;
    }

    public async Task<IEnumerable<RevolutTransaction2025>> ReadCsv(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var csvLines = await csv.GetRecordsAsync<RevolutTransaction2025CsvLine>().ToEnumerableAsync();
        return csvLines.Select(_revolutTransactionMapper.Map);
    }
}