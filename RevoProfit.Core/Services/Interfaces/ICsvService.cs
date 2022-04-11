using RevoProfit.Core.Models;

namespace RevoProfit.Core.Services.Interfaces;

public interface ICsvService
{
    Task<IEnumerable<Transaction>> ReadCsv(string path);
    Task<IEnumerable<Transaction>> ReadCsv(Stream stream);
}