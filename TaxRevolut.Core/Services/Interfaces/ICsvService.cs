using TaxRevolut.Core.Models;

namespace TaxRevolut.Core.Services.Interfaces;

public interface ICsvService
{
    IEnumerable<Transaction> ReadCsv(string path);
    IEnumerable<Transaction> ReadCsv(Stream stream);
}