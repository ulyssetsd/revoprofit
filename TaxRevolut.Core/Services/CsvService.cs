using System.Globalization;
using AutoMapper;
using CsvHelper;
using TaxRevolut.Core.Models;
using TaxRevolut.Core.Services.Interfaces;

namespace TaxRevolut.Core.Services;

public class CsvService : ICsvService
{
    private readonly Mapper _mapper;

    public CsvService()
    {
        _mapper = MapperFactory.GetMapper();
    }

    public IEnumerable<Transaction> ReadCsv(string path)
    {
        using var reader = new StreamReader(path);
        return ReadCsv(reader);
    }

    public IEnumerable<Transaction> ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return ReadCsv(reader);
    }

    private IEnumerable<Transaction> ReadCsv(TextReader streamReader)
    {
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var lastCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
        try
        {
            return csv.GetRecords<CsvLine>().Select(_mapper.Map<Transaction>).ToList();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }
    }
}