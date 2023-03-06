﻿using FluentAssertions;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoService : ICryptoService
{
    private const int EuroDecimalsPrecision = 24;

    private static CryptoAsset GetOrCreate(ICollection<CryptoAsset> cryptos, string symbol)
    {
        var crypto = cryptos.FirstOrDefault(crypto => crypto.Symbol == symbol);

        if (crypto == null)
        {
            crypto = new CryptoAsset
            {
                Symbol = symbol,
            };
            cryptos.Add(crypto);
        }

        return crypto;
    }

    private static void HandleFees(CryptoTransaction transaction, ICollection<CryptoAsset> cryptos, ICollection<CryptoFiatFee> fiatFees)
    {
        if (transaction.FeesAmount == 0)
        {
            return;
        }

        transaction.FeesAmount.Should().NotBe(0);
        transaction.FeesSymbol.Should().NotBeEmpty();

        if (transaction.FeesSymbol == "EUR")
        {
            fiatFees.Add(new CryptoFiatFee
            {
                Date = transaction.Date,
                FeesInEuros = transaction.FeesAmount,
            });
        }
        else
        {
            var feesCrypto = GetOrCreate(cryptos, transaction.FeesSymbol);
            feesCrypto.Fees += transaction.FeesAmount;
        }
    }

    private static void ResetIfEmpty(CryptoAsset crypto)
    {
        if (Math.Round(crypto.Amount, 14, MidpointRounding.ToEven) == 0)
        {
            crypto.Amount = 0;
            crypto.AmountInEuros = 0;
        }
    }

    public IEnumerable<CryptoReport> MapToReports(IEnumerable<CryptoSell> cryptoRetraits, IEnumerable<CryptoFiatFee> cryptoFiatFees)
    {
        var feesGroupByYear = cryptoFiatFees.GroupBy(fee => fee.Date.Year);
        var retraitsGroupByYear = cryptoRetraits.GroupBy(retrait => retrait.Date.Year);

        return feesGroupByYear
            .Join(retraitsGroupByYear, fees => fees.Key, retraits => retraits.Key, (fees, sells) => (year: fees.Key, fees, sells))
            .Select(joinGroupByYear => new CryptoReport
            {
                Year = joinGroupByYear.year,
                GainsInEuros = joinGroupByYear.sells.Sum(retrait => retrait.GainsInEuros),
                FeesInEuros = joinGroupByYear.fees.Sum(fee => fee.FeesInEuros),
            });
    }

    private static void Validate(CryptoTransaction transaction)
    {
        if (transaction.FeesAmount != 0)
        {
            transaction.FeesAmount.Should().NotBe(0);
            transaction.FeesSymbol.Should().NotBeEmpty();
        }

        switch (transaction.Type)
        {
            case CryptoTransactionType.Buy:
                transaction.BuyAmount.Should().NotBe(0);
                transaction.BuySymbol.Should().NotBeEmpty();
                transaction.BuyPrice.Should().NotBe(0);
                break;
            case CryptoTransactionType.Exchange:
                transaction.BuyAmount.Should().NotBe(0);
                transaction.BuySymbol.Should().NotBeEmpty();
                transaction.BuyPrice.Should().NotBe(0);

                transaction.SellAmount.Should().NotBe(0);
                transaction.SellSymbol.Should().NotBeEmpty();
                transaction.SellPrice.Should().NotBe(0);
                break;
            case CryptoTransactionType.Sell:
                transaction.SellAmount.Should().NotBe(0);
                transaction.SellSymbol.Should().NotBeEmpty();
                transaction.SellPrice.Should().NotBe(0);
                break;
        }
    }

    public (IReadOnlyCollection<CryptoAsset>, IReadOnlyCollection<CryptoSell>, IReadOnlyCollection<CryptoFiatFee>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions)
    {
        var cryptos = new List<CryptoAsset>();
        var sells = new List<CryptoSell>();
        var fiatFees = new List<CryptoFiatFee>();

        foreach (var transaction in transactions)
        {
            Validate(transaction);

            switch (transaction.Type)
            {
                case CryptoTransactionType.Buy:
                {
                    HandleFees(transaction, cryptos, fiatFees);

                    var inCrypto = GetOrCreate(cryptos, transaction.BuySymbol);
                    inCrypto.AmountInEuros += transaction.BuyPrice * transaction.BuyAmount;
                    inCrypto.Amount += transaction.BuyAmount;
                    break;
                }
                case CryptoTransactionType.Exchange:
                {
                    HandleFees(transaction, cryptos, fiatFees);

                    var outCrypto = GetOrCreate(cryptos, transaction.SellSymbol);

                    var outAveragePrice = outCrypto.AmountInEuros / outCrypto.Amount;
                    var insertedRatio = outAveragePrice / transaction.SellPrice;
                    var insertedAmount = transaction.SellAmount * insertedRatio;
                    var insertedAmountInEuros = transaction.SellPrice * insertedAmount;

                    outCrypto.Amount -= transaction.SellAmount;
                    outCrypto.AmountInEuros -= Math.Round(insertedAmountInEuros, EuroDecimalsPrecision, MidpointRounding.ToEven);
                    ResetIfEmpty(outCrypto);

                    var cryptoRecu = GetOrCreate(cryptos, transaction.BuySymbol);

                    cryptoRecu.Amount += transaction.BuyAmount;
                    cryptoRecu.AmountInEuros += Math.Round(insertedAmountInEuros, EuroDecimalsPrecision, MidpointRounding.ToEven);
                    break;
                }
                case CryptoTransactionType.Sell:
                {
                    HandleFees(transaction, cryptos, fiatFees);

                    var outCrypto = GetOrCreate(cryptos, transaction.SellSymbol);

                    var outAveragePrice = outCrypto.AmountInEuros / outCrypto.Amount;
                    var insertedRatio = outAveragePrice / transaction.SellPrice;
                    var gainsRatio = 1 - insertedRatio;

                    var gains = transaction.SellAmount * gainsRatio;
                    var gainsInEuros = gains * transaction.SellPrice;
                    var amountInEuros = transaction.SellAmount * transaction.SellPrice;

                    sells.Add(new CryptoSell
                    {
                        Date = transaction.Date,
                        Symbol = transaction.SellSymbol,
                        Amount = transaction.SellAmount,
                        AmountInEuros = Math.Round(amountInEuros, EuroDecimalsPrecision, MidpointRounding.ToEven),
                        GainsInEuros = Math.Round(gainsInEuros, EuroDecimalsPrecision, MidpointRounding.ToEven),
                        Price = transaction.SellPrice,
                    });

                    outCrypto.Amount -= transaction.SellAmount;
                    outCrypto.AmountInEuros -= Math.Round(amountInEuros * insertedRatio, EuroDecimalsPrecision, MidpointRounding.ToEven);
                    ResetIfEmpty(outCrypto);
                    break;
                }
                case CryptoTransactionType.FeesOnly:
                    HandleFees(transaction, cryptos, fiatFees);
                    break;
            }
        }

        return (cryptos, sells, fiatFees);
    }
}