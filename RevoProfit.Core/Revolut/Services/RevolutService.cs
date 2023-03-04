using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;

namespace RevoProfit.Core.Revolut.Services;

public class RevolutService : IRevolutService
{
    private readonly ICryptoService _cryptoService;

    public RevolutService(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<RevolutTransaction> transactions)
    {
        var cryptoTransactions = ConvertToCryptoTransactions(transactions);
        return _cryptoService.ProcessTransactions(cryptoTransactions);
    }

    public IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<RevolutTransaction> transactions)
    {
        return transactions.Where(IsNotTransactionToIgnore)
            .GroupBy(transaction => transaction.CompletedDate)
            .Select(HandleTransactionsGroupedByDate)
            .SelectMany(enumerable => enumerable)
            .OrderBy(transaction => transaction.Date);
    }

    private static bool IsNotTransactionToIgnore(RevolutTransaction transaction)
    {
        return transaction.Description is not "Balance migration to another region or legal entity" and not "Closing transaction";
    }

    private static IEnumerable<CryptoTransaction> HandleTransactionsGroupedByDate(IEnumerable<RevolutTransaction> revolutTransactions)
    {
        var sells = new List<RevolutTransaction>();
        var buys = new List<RevolutTransaction>();

        foreach (var transaction in revolutTransactions)
        {
            if (transaction.Amount < 0)
            {
                sells.Add(transaction);
            }
            if (transaction.Amount > 0)
            {
                buys.Add(transaction);
            }
            if (transaction.Amount == 0)
            {
                throw new ProcessException("A transaction with an empty amount was in the export");
            }
        }

        if (sells.Count > 1 || sells.Count == 1 && buys.Count > 1)
        {
            throw new ProcessException("More than two transactions on the same date time");
        }

        if (sells.Count == 0 && buys.Count > 0)
        {
            return buys.Select(buy =>
            {
                var buyPrice = buy.FiatAmount / buy.Amount;
                return new CryptoTransaction
                {
                    Type = CryptoTransactionType.Buy,
                    Date = buy.CompletedDate,

                    BuyAmount = buy.Amount,
                    BuySymbol = buy.Currency,
                    BuyPrice = buyPrice,

                    SellAmount = 0,
                    SellSymbol = string.Empty,
                    SellPrice = 0,

                    FeesAmount = buy.FiatFees,
                    FeesSymbol = buy.BaseCurrency,
                    FeesPrice = 1,
                };
            });
        }

        if (sells.Count == 1 && buys.Count == 0)
        {
            var sell = sells.First();
            var sellPrice = sell.FiatAmount / sell.Amount;
            return new[]
            {
                new CryptoTransaction
                {
                    Type = CryptoTransactionType.Sell,
                    Date = sell.CompletedDate,

                    BuyAmount = 0,
                    BuyPrice = 0,
                    BuySymbol = string.Empty,

                    SellAmount = -sell.Amount,
                    SellSymbol = sell.Currency,
                    SellPrice = sellPrice,

                    FeesAmount = sell.FiatFees,
                    FeesSymbol = sell.BaseCurrency,
                    FeesPrice = 1,
                },
            };
        }

        if (sells.Count == 1 && buys.Count == 1)
        {
            var sell = sells.First();
            var buy = buys.First();
            var sellPrice = sell.FiatAmount / sell.Amount;
            var buyPrice = buy.FiatAmount / buy.Amount;

            return new[]
            {
                new CryptoTransaction
                {
                    Type = CryptoTransactionType.FeesOnly,
                    Date = sell.CompletedDate,
                    BuyAmount = 0,
                    BuySymbol = string.Empty,
                    BuyPrice = 0,
                    SellAmount = 0,
                    SellSymbol = string.Empty,
                    SellPrice = 0,
                    FeesAmount = sell.FiatFees / sellPrice,
                    FeesSymbol = sell.Currency,
                    FeesPrice = sellPrice,
                },
                new CryptoTransaction
                {
                    Type = CryptoTransactionType.Exchange,
                    Date = sell.CompletedDate,

                    BuyAmount = buy.Amount,
                    BuySymbol = buy.Currency,
                    BuyPrice = buyPrice,

                    SellAmount = -sell.Amount,
                    SellSymbol = sell.Currency,
                    SellPrice = sellPrice,

                    FeesAmount = buy.FiatFees / buyPrice,
                    FeesSymbol = buy.Currency,
                    FeesPrice = buyPrice,
                },
            };
        }

        throw new ProcessException("Transactions without correct format");
    }
}