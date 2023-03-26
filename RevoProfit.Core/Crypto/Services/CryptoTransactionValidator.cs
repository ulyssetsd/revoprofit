using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoTransactionValidator : ICryptoTransactionValidator
{
    public void ValidateAndThrow(CryptoTransaction transaction)
    {
        if (!IsValid(transaction)) throw new ProcessException($"Invalid transaction: {transaction}");
    }

    public bool IsValid(CryptoTransaction transaction)
    {
        if (transaction.FeesAmount > 0 && !IsValidFees(transaction)) return false;
        return transaction.Type switch
        {
            CryptoTransactionType.Buy => IsValidBuy(transaction),
            CryptoTransactionType.Sell => IsValidSell(transaction),
            CryptoTransactionType.Exchange => IsValidBuy(transaction) && IsValidSell(transaction),
            CryptoTransactionType.FeesOnly => true,
            _ => false,
        };
    }

    private static bool IsValidBuy(CryptoTransaction transaction) =>
        transaction.BuyAmount > 0 &&
        transaction.BuyPrice > 0 &&
        !string.IsNullOrEmpty(transaction.BuySymbol);

    private static bool IsValidSell(CryptoTransaction transaction) =>
        transaction.SellAmount > 0 &&
        transaction.SellPrice > 0 &&
        !string.IsNullOrEmpty(transaction.SellSymbol);

    private static bool IsValidFees(CryptoTransaction transaction) =>
        transaction.FeesPrice > 0 &&
        !string.IsNullOrEmpty(transaction.FeesSymbol);
}