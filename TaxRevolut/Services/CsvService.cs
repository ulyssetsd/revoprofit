using System.Globalization;
using AutoMapper;
using CsvHelper;
using TaxRevolut.Models;

namespace TaxRevolut.Services;

public class CsvService
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

        return csv.GetRecords<CsvLine>().Select(_mapper.Map<Transaction>);
    }
}