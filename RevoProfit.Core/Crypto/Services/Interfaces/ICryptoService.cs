using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface ICryptoService
{
    (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoRetrait>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions);
    IEnumerable<CryptoReport> MapToReports(IEnumerable<CryptoRetrait> cryptoRetraits, IEnumerable<CryptoFiatFee> cryptoFiatFees);
}