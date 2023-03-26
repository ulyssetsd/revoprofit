using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface ICryptoTransactionValidator
{
    void ValidateAndThrow(CryptoTransaction instance);
    bool IsValid(CryptoTransaction instance);
}