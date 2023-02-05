using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services.Interfaces;
using System.Linq;

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
            .SelectMany(enumerable => enumerable);
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
                    MontantRecu = (double)depot.Amount,
                    MonnaieOuJetonRecu = depot.Currency,
                    PrixDuJetonDuMontantRecu = (double)currencyPrice,
                    MontantEnvoye = 0,
                    MonnaieOuJetonEnvoye = null,
                    PrixDuJetonDuMontantEnvoye = 0,
                    Frais = (double)depot.Fee,
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
                    MonnaieOuJetonRecu = null,
                    PrixDuJetonDuMontantRecu = 0,
                    MontantEnvoye = (double)-retrait.Amount,
                    MonnaieOuJetonEnvoye = retrait.Currency,
                    PrixDuJetonDuMontantEnvoye = (double)currencyPrice,
                    Frais = (double)retrait.Fee,
                    MonnaieOuJetonDesFrais = retrait.BaseCurrency,
                    PrixDuJetonDesFrais = 1,
                }
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

                    MontantRecu = (double)depot.Amount,
                    MonnaieOuJetonRecu = depot.Currency,
                    PrixDuJetonDuMontantRecu = (double)targetCurrencyPrice,

                    MontantEnvoye = (double)-retrait.Amount,
                    MonnaieOuJetonEnvoye = retrait.Currency,
                    PrixDuJetonDuMontantEnvoye = (double)sourceCurrencyPrice,

                    Frais = (double)(totalFee / targetCurrencyPrice),
                    MonnaieOuJetonDesFrais = depot.Currency,
                    PrixDuJetonDesFrais = (double)targetCurrencyPrice,
                }
            };
        }

        throw new ProcessException("Transactions without correct format");
    }
}