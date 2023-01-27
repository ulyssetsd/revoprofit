using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface ICryptoService
{
    (List<CryptoAsset>, List<CryptoRetrait>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions);
    IEnumerable<CryptoReport> MapToReports(IEnumerable<CryptoRetrait> cryptoRetraits);
}