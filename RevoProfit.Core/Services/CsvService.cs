using AutoMapper;
using CsvHelper;
using RevoProfit.Core.Models;
using RevoProfit.Core.Services.Interfaces;
using System.Globalization;

namespace RevoProfit.Core.Services;

public class CsvService : ICsvService
{
    private readonly Mapper _mapper;

    public CsvService()
    {
        _mapper = MapperFactory.GetMapper();
    }

    public async Task<IEnumerable<Transaction>> ReadCsv(string path)
    {
        using var reader = new StreamReader(path);
        return await ReadCsv(reader);
    }

    public async Task<IEnumerable<Transaction>> ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await ReadCsv(reader);
    }

    private async Task<IEnumerable<Transaction>> ReadCsv(TextReader streamReader)
    {
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var lastCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
        try
        {
            var csvLines = await csv.GetRecordsAsync<CsvLine>().ToEnumerableAsync();
            return csvLines.Select(_mapper.Map<Transaction>).ToList();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }
    }
}

public static class EnumerableAsyncExtensions
{
    public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<T>();
        await foreach (var item in asyncEnumerable)
        {
            list.Add(item);
        }

        return list;
    }
}