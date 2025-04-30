using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut2025.Models;
using RevoProfit.Core.Revolut2025.Services.Interfaces;

namespace RevoProfit.Core.Revolut2025.Services;

public class Revolut2025Service : IRevolut2025Service
{
    private readonly ICryptoService _cryptoService;
    private readonly ICurrencyService _currencyService;

    public Revolut2025Service(ICryptoService cryptoService, ICurrencyService currencyService)
    {
        _cryptoService = cryptoService;
        _currencyService = currencyService;
    }

    public (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<Revolut2025Transaction> transactions)
    {
        var cryptoTransactions = ConvertToCryptoTransactions(transactions);
        return _cryptoService.ProcessTransactions(cryptoTransactions);
    }

    private IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<Revolut2025Transaction> transactions)
    {
        var result = new List<CryptoTransaction>();

        foreach (var transaction in transactions)
        {
            if (transaction.Fees > 0)
            {
                result.Add(new CryptoTransaction
                {
                    Type = CryptoTransactionType.FeesOnly,
                    Date = transaction.Date,

                    BuyAmount = 0,
                    BuySymbol = string.Empty,
                    BuyPrice = 0,

                    SellAmount = 0,
                    SellSymbol = string.Empty,
                    SellPrice = 0,

                    FeesAmount = transaction.FeesInEur(_currencyService),
                    FeesSymbol = "EUR",
                    FeesPrice = 1
                });
            }
        }

        foreach (var groupedTransactions in transactions.GroupBy(t => t.Date))
        {
            var groupDate = groupedTransactions.Key;
            var transactionsList = groupedTransactions.ToList();

            var sells = transactionsList.Where(t => t.Type == Revolut2025TransactionType.Sell).ToList();
            var buys = transactionsList.Where(t => t.Type == Revolut2025TransactionType.Buy).ToList();

            if (sells.Count == 1 && buys.Count == 1)
            {
                var sell = sells[0];
                var buy = buys[0];

                result.Add(new CryptoTransaction
                {
                    Type = CryptoTransactionType.Exchange,
                    Date = groupDate,

                    SellAmount = sell.Quantity,
                    SellSymbol = sell.Symbol,
                    SellPrice = sell.PriceInEur(_currencyService),

                    BuyAmount = buy.Quantity,
                    BuySymbol = buy.Symbol,
                    BuyPrice = buy.PriceInEur(_currencyService),

                    FeesAmount = 0,
                    FeesSymbol = string.Empty,
                    FeesPrice = 0
                });

                transactionsList.Remove(sell);
                transactionsList.Remove(buy);
            }
            else if (sells.Count > 1 && buys.Count > 0)
            {
                throw new ProcessException($"Multiple sell transactions for the same date are not supported. Date: {groupDate:MMM d, yyyy, h:mm:ss tt}");
            }
            else if (sells.Count > 0 && buys.Count > 1)
            {
                throw new ProcessException($"Multiple buy transactions for the same date are not supported. Date: {groupDate:MMM d, yyyy, h:mm:ss tt}");
            }

            foreach (var transaction in transactionsList)
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
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
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
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
                        });
                        break;

                    case Revolut2025TransactionType.Buy:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.PriceInEur(_currencyService),

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
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
                            SellPrice = transaction.PriceInEur(_currencyService),

                            BuyAmount = 0,
                            BuySymbol = string.Empty,
                            BuyPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
                        });
                        break;

                    case Revolut2025TransactionType.Stake:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Exchange,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = transaction.Symbol,
                            SellPrice = transaction.PriceInEur(_currencyService),

                            BuyAmount = transaction.Quantity,
                            BuySymbol = $"{transaction.Symbol}_STAKE",
                            BuyPrice = transaction.PriceInEur(_currencyService),

                            FeesAmount = 0,
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
                        });
                        break;

                    case Revolut2025TransactionType.Unstake:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Exchange,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = $"{transaction.Symbol}_STAKE",
                            SellPrice = transaction.PriceInEur(_currencyService),

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.PriceInEur(_currencyService),

                            FeesAmount = 0,
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
                        });
                        break;

                    case Revolut2025TransactionType.Other:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.PriceInEur(_currencyService),

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = string.Empty,
                            FeesPrice = 0
                        });
                        break;

                    case Revolut2025TransactionType.Send:
                    case Revolut2025TransactionType.Receive:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(transaction.Type), transaction.Type, null);
                }
            }
        }

        return result.OrderBy(t => t.Date);
    }
}