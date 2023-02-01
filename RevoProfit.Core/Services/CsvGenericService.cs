using System.Globalization;
using CsvHelper;
using RevoProfit.Core.Extensions;
using RevoProfit.Core.Services.Intefaces;

namespace RevoProfit.Core.Services;

public abstract class CsvGenericService<T, TCsv> : ICsvService<T>
{
    public async Task<IEnumerable<T>> ReadCsv(string path)
    {
        using var reader = new StreamReader(path);
        return await ReadCsv(reader);
    }

    public async Task<IEnumerable<T>> ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await ReadCsv(reader);
    }

    private async Task<IEnumerable<T>> ReadCsv(TextReader streamReader)
    {
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var csvLines = await csv.GetRecordsAsync<TCsv>().ToEnumerableAsync();
        return csvLines.Select(MapCsvLineToModel).ToList();
    }

    public abstract T MapCsvLineToModel(TCsv source);
}