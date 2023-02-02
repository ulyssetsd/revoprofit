using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface ICryptoCsvService
{
    Task<IEnumerable<CryptoTransaction>> ReadCsv(Stream stream);
}