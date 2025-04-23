using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;
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
        var result = new List<CryptoTransaction>();

        foreach (var groupedTransactions in transactions.GroupBy(t => t.Date))
        {
            var groupDate = groupedTransactions.Key;

            foreach (var transaction in groupedTransactions)
            {
                switch (transaction.Type)
                {
                    case RevolutTransactionType.LearnReward:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? 1m,

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = "EUR",
                            FeesPrice = 1
                        });
                        break;

                    case RevolutTransactionType.StakingReward:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? 1m,

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = 0,
                            FeesSymbol = "EUR",
                            FeesPrice = 1
                        });
                        break;

                    case RevolutTransactionType.Buy:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Buy,
                            Date = transaction.Date,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? 1m,

                            SellAmount = 0,
                            SellSymbol = string.Empty,
                            SellPrice = 0,

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for buy transactions"),
                            FeesSymbol = transaction.FeesCurrency ?? throw new ProcessException("FeesCurrency cannot be null for buy transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case RevolutTransactionType.Sell:
                    case RevolutTransactionType.Payment:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Sell,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = transaction.Symbol,
                            SellPrice = transaction.Price ?? 1m,

                            BuyAmount = 0,
                            BuySymbol = string.Empty,
                            BuyPrice = 0,

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for sell transactions"),
                            FeesSymbol = transaction.PriceCurrency ?? throw new ProcessException("FeesCurrency cannot be null for sell transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case RevolutTransactionType.Stake:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Exchange,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = transaction.Symbol,
                            SellPrice = transaction.Price ?? 1m,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = $"{transaction.Symbol}_STAKE",
                            BuyPrice = transaction.Price ?? 1m,

                            FeesAmount = transaction.Fees ?? throw new ProcessException("Fees cannot be null for exchange transactions"),
                            FeesSymbol = transaction.PriceCurrency ?? throw new ProcessException("FeesCurrency cannot be null for exchange transactions"),
                            FeesPrice = 1
                        });
                        break;

                    case RevolutTransactionType.Unstake:
                        result.Add(new CryptoTransaction
                        {
                            Type = CryptoTransactionType.Exchange,
                            Date = transaction.Date,

                            SellAmount = transaction.Quantity,
                            SellSymbol = $"{transaction.Symbol}_STAKE",
                            SellPrice = transaction.Price ?? 1m,

                            BuyAmount = transaction.Quantity,
                            BuySymbol = transaction.Symbol,
                            BuyPrice = transaction.Price ?? 1m,

                            FeesAmount = transaction.Fees ?? 0,
                            FeesSymbol = transaction.PriceCurrency ?? "EUR",
                            FeesPrice = 1
                        });
                        break;

                    case RevolutTransactionType.Send:
                    case RevolutTransactionType.Receive:
                        // Ignore send and receive transactions
                        break;
                }
            }
        }

        return result.OrderBy(t => t.Date);
    }
}