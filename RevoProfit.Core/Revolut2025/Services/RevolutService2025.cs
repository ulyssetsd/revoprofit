using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Revolut2025.Services;

public interface IRevolutService2025
{
    (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<RevolutTransaction2025> transactions);
}


public class RevolutService2025 : IRevolutService2025
{
    private readonly ICryptoService _cryptoService;

    public RevolutService2025(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<RevolutTransaction2025> transactions)
    {
        var cryptoTransactions = ConvertToCryptoTransactions(transactions);
        return _cryptoService.ProcessTransactions(cryptoTransactions);
    }

    private IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<RevolutTransaction2025> transactions)
    {
        throw new NotImplementedException("This method is not implemented yet.");
    }
}