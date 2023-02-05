using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
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
        return _cryptoService.ProcessTransactions(ConvertToCryptoTransactions(transactions));
    }

    public IEnumerable<CryptoTransaction> ConvertToCryptoTransactions(IEnumerable<RevolutTransaction> transactions)
    {
        return transactions.Where(transaction => transaction.Description != "Balance migration to another region or legal entity")
            .GroupBy(transaction => transaction.CompletedDate)
            .Select(HandleTransactionsGroupedByDate)
            .ToList();
    }

    private static CryptoTransaction HandleTransactionsGroupedByDate(IEnumerable<RevolutTransaction> revolutTransactions)
    {
        RevolutTransaction? retrait = null;
        RevolutTransaction? depot = null;
        var count = 0;
        foreach (var transaction in revolutTransactions)
        {
            count++;
            if (transaction.Amount < 0)
            {
                retrait ??= transaction;
            }
            if (transaction.Amount > 0)
            {
                depot ??= transaction;
            }
        }

        if (new[] { retrait != null, depot != null }.Count(t => true) < count)
        {
            throw new Exception("more than two transactions on the same date time");
        }

        if (retrait == null && depot != null)
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
        }

        if (depot == null && retrait != null)
        {
            var currencyPrice = retrait.FiatAmount / retrait.Amount;
            return new CryptoTransaction
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
            };
        }

        if (depot != null && retrait != null)
        {
            var totalFee = retrait.Fee + depot.Fee;
            var sourceCurrencyPrice = retrait.FiatAmountIncludingFees / retrait.Amount;
            var targetCurrencyPrice = depot.FiatAmount / depot.Amount;

            return new CryptoTransaction
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
            };
        }

        throw new Exception("transactions without correct format");
    }
}