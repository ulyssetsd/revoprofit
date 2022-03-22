using RevoProfit.Core.Models;

namespace RevoProfit.Core.Services.Interfaces;

public interface ICsvService
{
    IEnumerable<Transaction> ReadCsv(string path);
    IEnumerable<Transaction> ReadCsv(Stream stream);
}