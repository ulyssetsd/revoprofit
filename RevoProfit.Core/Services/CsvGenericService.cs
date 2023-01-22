using System.Globalization;
using AutoMapper;
using CsvHelper;
using RevoProfit.Core.Extensions;
using RevoProfit.Core.Mapping;
using RevoProfit.Core.Services.Intefaces;

namespace RevoProfit.Core.Services;

public class CsvGenericService<T, TCsv> : ICsvService<T>
{
    private readonly Mapper _mapper;
    private readonly CultureInfo _cultureInfo;

    public CsvGenericService(CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
        _mapper = MapperFactory.GetMapper();
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
            return csvLines.Select(source => _mapper.Map<T>(source)).ToList();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }
    }
}