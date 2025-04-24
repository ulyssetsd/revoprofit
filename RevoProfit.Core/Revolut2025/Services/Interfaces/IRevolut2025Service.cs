using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Revolut2025.Models;

namespace RevoProfit.Core.Revolut2025.Services.Interfaces;

public interface IRevolut2025Service
{
    (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<Revolut2025Transaction> transactions);
}
