using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut2025.Models;
using RevoProfit.Core.Revolut2025.Services.Interfaces;

namespace RevoProfit.Core.Revolut2025.Services;

public class Revolut2025Service : IRevolut2025Service
{
    private readonly ICryptoService _cryptoService;

    public Revolut2025Service(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<Revolut2025Transaction> transactions)
    {
        var cryptoTransactions = ConvertToCryptoTransactions(transactions);
        return _cryptoService.ProcessTransactions(cryptoTransactions);
    }

    private IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<Revolut2025Transaction> transactions)
    {
        var result = new List<CryptoTransaction>();

        foreach (var groupedTransactions in transactions.GroupBy(t => t.Date))
        {
            var groupDate = groupedTransactions.Key;

            foreach (var transaction in groupedTransactions)
            {
                switch (transaction.Type)
                {
                    case Revolut2025TransactionType.LearnReward:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = 0,

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = "EUR",
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.StakingReward:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = 0,

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = "EUR",
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.Buy:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for buy transactions"),

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for buy transactions"),
                            FeesSymbol = transaction.FeesCurrency ?? throw new ProcessException("FeesCurrency cannot be null for buy transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.Sell:
                    case Revolut2025TransactionType.Payment:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Sell,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = transaction.Symbol,
                            SellPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for sell transactions"),

                            BuyAmount = 0,
                            BuySymbol = string.Empty,
                            BuyPrice = 0,

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for sell transactions"),
                            FeesSymbol = transaction.PriceCurrency ?? throw new ProcessException("FeesCurrency cannot be null for sell transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.Stake:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Exchange,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = transaction.Symbol,
                            SellPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for exchange transactions"),

                            BuyAmount = transaction.Quantity,
                            BuySymbol = $"{transaction.Symbol}_STAKE",
                            BuyPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for exchange transactions"),

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for exchange transactions"),
                            FeesSymbol = transaction.PriceCurrency ?? throw new ProcessException("FeesCurrency cannot be null for exchange transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.Unstake:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Exchange,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = $"{transaction.Symbol}_STAKE",
                            SellPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for exchange transactions"),

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for exchange transactions"),

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for exchange transactions"),
                            FeesSymbol = transaction.PriceCurrency ?? throw new ProcessException("FeesCurrency cannot be null for exchange transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.Other:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? throw new ProcessException("Price cannot be null for other transactions"),

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for other transactions"),
                            FeesSymbol = transaction.FeesCurrency ?? throw new ProcessException("FeesCurrency cannot be null for other transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case Revolut2025TransactionType.Send:
                    case Revolut2025TransactionType.Receive:
                        // Ignore send and receive transactions
                        break;
                }
            }
        }

        return result.OrderBy(t => t.Date);
    }
}