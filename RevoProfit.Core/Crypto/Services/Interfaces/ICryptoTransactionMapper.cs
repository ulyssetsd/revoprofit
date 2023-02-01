using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface ICryptoTransactionMapper
{
    CryptoTransaction Map(CryptoTransactionCsvLine source);
}