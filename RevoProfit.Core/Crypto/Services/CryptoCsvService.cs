using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Services;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoCsvService : CsvGenericService<CryptoTransaction, CryptoTransactionCsvLine>, ICryptoCsvService
{
    private readonly ICryptoTransactionMapper _cryptoTransactionMapper;

    public CryptoCsvService(ICryptoTransactionMapper cryptoTransactionMapper)
    {
        _cryptoTransactionMapper = cryptoTransactionMapper;
    }

    public override CryptoTransaction MapCsvLineToModel(CryptoTransactionCsvLine source)
    {
        return _cryptoTransactionMapper.Map(source);
    }
}