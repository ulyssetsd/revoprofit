using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface ICryptoService
{
    (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions);
    IEnumerable<CryptoReport> MapToReports(IEnumerable<CryptoSell> cryptoRetraits, IEnumerable<CryptoFiatFee> cryptoFiatFees);
}