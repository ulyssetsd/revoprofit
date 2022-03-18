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
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<CsvLine>().Select(_mapper.Map<Transaction>).ToList();
    }

    public IEnumerable<Transaction> ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<CsvLine>().Select(_mapper.Map<Transaction>).ToList();
    }
}