using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Revolut.Models;

namespace RevoProfit.Core.Revolut.Services.Interfaces;

public interface IRevolutService
{
    IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<RevolutTransaction> transactions);
    (IEnumerable<CryptoAsset>, IEnumerable<CryptoRetrait>) ProcessTransactions(IEnumerable<RevolutTransaction> transactions);
}