using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Revolut.Models;

namespace RevoProfit.Core.Revolut.Services.Interfaces;

public interface IRevolutService
{
    IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<RevolutTransaction> transactions);
    (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<RevolutTransaction> transactions);
}