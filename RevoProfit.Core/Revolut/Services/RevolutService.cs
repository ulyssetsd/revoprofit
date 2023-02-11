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

    public (IEnumerable<CryptoAsset>, IEnumerable<CryptoRetrait>) ProcessTransactions(IEnumerable<RevolutTransaction> transactions)
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
        var retraits = new List<RevolutTransaction>();
        var depots = new List<RevolutTransaction>();
        var count = 0;
        foreach (var transaction in revolutTransactions)
        {
            count++;
            if (transaction.Amount < 0)
            {
                retraits.Add(transaction);
            }
            if (transaction.Amount > 0)
            {
                depots.Add(transaction);
            }
            if (transaction.Amount == 0)
            {
                throw new ProcessException("A transaction with an empty amount was in the export");
            }
        }

        if (retraits.Count > 1 || retraits.Count == 1 && depots.Count > 1)
        {
            throw new ProcessException("More than two transactions on the same date time");
        }

        if (retraits.Count == 0 && depots.Count > 0)
        {
            return depots.Select(depot =>
            {
                var currencyPrice = depot.FiatAmount / depot.Amount;
                return new CryptoTransaction
                {
                    Type = CryptoTransactionType.Depot,
                    Date = depot.CompletedDate,

                    MontantRecu = depot.Amount,
                    MonnaieOuJetonRecu = depot.Currency,
                    PrixDuJetonDuMontantRecu = currencyPrice,

                    MontantEnvoye = 0,
                    MonnaieOuJetonEnvoye = string.Empty,
                    PrixDuJetonDuMontantEnvoye = 0,

                    Frais = depot.Fee,
                    MonnaieOuJetonDesFrais = depot.BaseCurrency,
                    PrixDuJetonDesFrais = 1,
                };
            });
        }

        if (retraits.Count == 1 && depots.Count == 0)
        {
            var retrait = retraits.First();
            var currencyPrice = retrait.FiatAmount / retrait.Amount;
            return new[]
            {
                new CryptoTransaction
                {
                    Type = CryptoTransactionType.Retrait,
                    Date = retrait.CompletedDate,

                    MontantRecu = 0,
                    PrixDuJetonDuMontantRecu = 0,
                    MonnaieOuJetonRecu = string.Empty,

                    MontantEnvoye = -retrait.Amount,
                    MonnaieOuJetonEnvoye = retrait.Currency,
                    PrixDuJetonDuMontantEnvoye = currencyPrice,

                    Frais = retrait.Fee,
                    MonnaieOuJetonDesFrais = retrait.BaseCurrency,
                    PrixDuJetonDesFrais = 1,
                },
            };
        }

        if (retraits.Count == 1 && depots.Count == 1)
        {
            var retrait = retraits.First();
            var depot = depots.First();
            var totalFee = retrait.Fee + depot.Fee;
            var sourceCurrencyPrice = retrait.FiatAmountIncludingFees / retrait.Amount;
            var targetCurrencyPrice = depot.FiatAmount / depot.Amount;

            return new[]
            {
                new CryptoTransaction
                {
                    Type = CryptoTransactionType.Echange,
                    Date = retrait.CompletedDate,

                    MontantRecu = depot.Amount,
                    MonnaieOuJetonRecu = depot.Currency,
                    PrixDuJetonDuMontantRecu = targetCurrencyPrice,

                    MontantEnvoye = -retrait.Amount,
                    MonnaieOuJetonEnvoye = retrait.Currency,
                    PrixDuJetonDuMontantEnvoye = sourceCurrencyPrice,

                    Frais = totalFee / targetCurrencyPrice,
                    MonnaieOuJetonDesFrais = depot.Currency,
                    PrixDuJetonDesFrais = targetCurrencyPrice,
                },
            };
        }

        throw new ProcessException("Transactions without correct format");
    }
}