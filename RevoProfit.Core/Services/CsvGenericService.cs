using System.Globalization;
using CsvHelper;
using RevoProfit.Core.Extensions;
using RevoProfit.Core.Services.Intefaces;

namespace RevoProfit.Core.Services;

public abstract class CsvGenericService<T, TCsv> : ICsvService<T>
{
    private readonly CultureInfo _cultureInfo;

    protected CsvGenericService(CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
    }

    public async Task<IEnumerable<T>> ReadCsv(string path)
    {
        using var reader = new StreamReader(path);
        return await ReadCsv(reader, _cultureInfo);
    }

    public async Task<IEnumerable<T>> ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await ReadCsv(reader, _cultureInfo);
    }

    private async Task<IEnumerable<T>> ReadCsv(TextReader streamReader, CultureInfo cultureInfo)
    {
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var lastCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        try
        {
            var csvLines = await csv.GetRecordsAsync<TCsv>().ToEnumerableAsync();
            return csvLines.Select(MapCsvLineToModel).ToList();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }
    }

    public abstract T MapCsvLineToModel(TCsv source);
}