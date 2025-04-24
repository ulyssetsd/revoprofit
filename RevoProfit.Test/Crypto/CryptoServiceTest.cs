using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services;

namespace RevoProfit.Test.Crypto;

public class CryptoServiceTest
{
    private CryptoService _cryptoService = null!;
    private int _dateIncrement;
    private const string Bitcoin = "BTC";
    private const string Ethereum = "ETH";
    private const decimal DefaultUsdToEurRate = 0.91m;

    [SetUp]
    public void Setup()
    {
        _cryptoService = new CryptoService(new CryptoTransactionFluentValidator(), new MockExchangeRateProvider(DefaultUsdToEurRate));
        _dateIncrement = 0;
    }

    private CryptoTransaction Btc(CryptoTransactionType cryptoTransactionType, decimal price, decimal quantity = 1, int yearIncrement = 0, decimal ethPrice = 1) =>
        CryptoTransaction(cryptoTransactionType, price, quantity, yearIncrement, ethPrice, Bitcoin, Ethereum);

    private CryptoTransaction Eth(CryptoTransactionType cryptoTransactionType, decimal price, decimal quantity = 1, int yearIncrement = 0, decimal btcPrice = 1) =>
        CryptoTransaction(cryptoTransactionType, price, quantity, yearIncrement, btcPrice, Ethereum, Bitcoin);

    private CryptoTransaction CryptoTransaction(CryptoTransactionType cryptoTransactionType, decimal price, decimal quantity, int yearIncrement, decimal destinationPrice, string source, string destination)
    {
        var date = DateTime.Today.AddYears(yearIncrement).AddDays(++_dateIncrement);

        return cryptoTransactionType switch
        {
            CryptoTransactionType.Buy => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                BuySymbol = source,
                BuyPrice = price,
                BuyAmount = quantity,
                SellAmount = 0,
                SellSymbol = string.Empty,
                SellPrice = 0,
                FeesAmount = 0,
                FeesSymbol = string.Empty,
                FeesPrice = 0,
            },
            CryptoTransactionType.Sell => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                SellSymbol = source,
                SellPrice = price,
                SellAmount = quantity,
                BuyAmount = 0,
                BuySymbol = string.Empty,
                BuyPrice = 0,
                FeesAmount = 0,
                FeesSymbol = string.Empty,
                FeesPrice = 0,
            },
            CryptoTransactionType.Exchange => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                SellSymbol = source,
                SellPrice = price,
                SellAmount = quantity,
                BuySymbol = destination,
                BuyPrice = destinationPrice,
                BuyAmount = quantity * price / destinationPrice,
                FeesAmount = 0,
                FeesSymbol = string.Empty,
                FeesPrice = 0,
            },
            _ => throw new NotImplementedException(),
        };
    }

    [Test]
    public void TestCalculDesGainsSansEchange()
    {
        var transactions = new List<CryptoTransaction>
        {
            Btc(CryptoTransactionType.Buy, price: 100, quantity: 1),
            Btc(CryptoTransactionType.Sell, price: 200, quantity: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.Single().Should().BeEquivalentTo(new CryptoSell
        {
            Amount = 0.5m,
            AmountInEuros = 100,
            GainsInEuros = 50,
            Symbol = Bitcoin,
            Price = 200,
            Date = transactions[1].Date,
        });

        cryptoAssets.Single().Should().BeEquivalentTo(new CryptoAsset
        {
            Symbol = Bitcoin,
            Amount = 0.5m,
            AmountInEuros = 50,
        });
    }

    [Test]
    public void TestCalculDesGainsAvecEchangeEtPrixSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Btc(CryptoTransactionType.Buy, price: 100),
            Btc(CryptoTransactionType.Exchange, price: 100, quantity: .5m, ethPrice: 100),
            Btc(CryptoTransactionType.Sell, price: 200, quantity: .5m),
            Eth(CryptoTransactionType.Sell, price: 100, quantity: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.Should().BeEquivalentTo(new List<CryptoSell>
        {
            new()
            {
                Amount = 0.5m,
                AmountInEuros = 100,
                GainsInEuros = 50,
                Symbol = Bitcoin,
                Price = 200,
                Date = transactions[2].Date,
            },
            new()
            {
                Amount = 0.5m,
                AmountInEuros = 50,
                GainsInEuros = 0,
                Symbol = Ethereum,
                Price = 100,
                Date = transactions[3].Date,
            },
        });

        cryptoAssets.Should().BeEquivalentTo(new List<CryptoAsset>
        {
            new()
            {
                Symbol = Bitcoin,
                Amount = 0,
                AmountInEuros = 0,
            },
            new()
            {
                Symbol = Ethereum,
                Amount = 0,
                AmountInEuros = 0,
            },
        });
    }

    [Test]
    public void TestCalculDesGainsAvecEchangeEtPrixNonSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Btc(CryptoTransactionType.Buy, price: 100),
            Btc(CryptoTransactionType.Exchange, price: 200, quantity: .5m, ethPrice: 100),
            Eth(CryptoTransactionType.Sell, price: 200, quantity: 1),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.Should().BeEquivalentTo(new List<CryptoSell>
        {
            new()
            {
                Amount = 1,
                AmountInEuros = 200,
                GainsInEuros = 150,
                Symbol = Ethereum,
                Price = 200,
                Date = transactions[2].Date,
            },
        });
        cryptoAssets.Should().BeEquivalentTo(new List<CryptoAsset>
        {
            new()
            {
                Symbol = Bitcoin,
                Amount = 0.5m,
                AmountInEuros = 50,
            },
            new()
            {
                Symbol = Ethereum,
                Amount = 0,
                AmountInEuros = 0,
            },
        });
    }

    [Test]
    public void TestCalculDesGainsAvecPlusieursRetraitsEtEchangesEtPrixNonSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Btc(CryptoTransactionType.Buy, price: 100),
            Btc(CryptoTransactionType.Exchange, price: 200, quantity: .5m, ethPrice: 100),
            Eth(CryptoTransactionType.Sell, price: 200, quantity: 1),
            Btc(CryptoTransactionType.Sell, price: 300, quantity: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);
        
        retraits.Should().BeEquivalentTo(new List<CryptoSell>
        {
            new()
            {
                Amount = 0.5m,
                AmountInEuros = 150,
                GainsInEuros = 100,
                Symbol = Bitcoin,
                Price = 300,
                Date = transactions[3].Date,
            },
            new()
            {
                Amount = 1,
                AmountInEuros = 200,
                GainsInEuros = 150,
                Symbol = Ethereum,
                Price = 200,
                Date = transactions[2].Date,
            },
        });
        cryptoAssets.Should().BeEquivalentTo(new List<CryptoAsset>
        {
            new()
            {
                Symbol = Bitcoin,
                Amount = 0,
                AmountInEuros = 0,
            },
            new()
            {
                Symbol = Ethereum,
                Amount = 0,
                AmountInEuros = 0,
            },
        });
    }

    [Test]
    public void TestCalculDesGainsAvecPlusieursRetraitsEtEchangesEtPrixNonSimilaires2()
    {
        var transactions = new List<CryptoTransaction>
        {
            Btc(CryptoTransactionType.Buy, price: 100),
            Btc(CryptoTransactionType.Exchange, price: 200, quantity: .5m, ethPrice: 100),
            Eth(CryptoTransactionType.Buy, price: 200, quantity: 1),
            Eth(CryptoTransactionType.Sell, price: 300, quantity: 2),
            Btc(CryptoTransactionType.Sell, price: 300, quantity: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);
        
        retraits.Should().BeEquivalentTo(new List<CryptoSell>
            {
                new()
                {
                    Amount = 0.5m,
                    AmountInEuros = 150,
                    GainsInEuros = 100,
                    Symbol = Bitcoin,
                    Price = 300,
                    Date = transactions[4].Date,
                },
                new()
                {
                    Amount = 2,
                    AmountInEuros = 600,
                    GainsInEuros = 350,
                    Symbol = Ethereum,
                    Price = 300,
                    Date = transactions[3].Date,
                },
            });
        cryptoAssets.Should().BeEquivalentTo(new List<CryptoAsset>
        {
            new()
            {
                Symbol = Bitcoin,
                Amount = 0,
                AmountInEuros = 0,
            },
            new()
            {
                Symbol = Ethereum,
                Amount = 0,
                AmountInEuros = 0,
            },
        });
    }

    [Test]
    public void TestCalculDesFraisEnEuros()
    {
        var feeSymbol = "EUR";
        var feeAmount = 10m;
        IEnumerable<CryptoTransaction> transactions = [
            Btc(CryptoTransactionType.Buy, price: 100, quantity: 1),
            Btc(CryptoTransactionType.Sell, price: 200, quantity: .5m) with
            {
                FeesAmount = feeAmount,
                FeesSymbol = feeSymbol,
                FeesPrice = 1,
            },
        ];

        var (_, _, fiatFees) = _cryptoService.ProcessTransactions(transactions);

        fiatFees.Should().BeEquivalentTo(
        [
            new CryptoFiatFee()
            {
                Date = transactions.ElementAt(1).Date,
                FeesInEuros = feeAmount,
            },
        ]);
    }

    [Test]
    public void TestCalculDesFraisEnUSD()
    {
        var feeSymbol = "USD";
        var feeAmount = 10m;
        var expectedEuroAmount = Math.Round(feeAmount * DefaultUsdToEurRate, 2, MidpointRounding.ToEven);
        var transactionDate = DateTime.Today;
        IEnumerable<CryptoTransaction> transactions = [
            Btc(CryptoTransactionType.Buy, price: 100, quantity: 1),
            Btc(CryptoTransactionType.Sell, price: 200, quantity: .5m) with
            {
                FeesAmount = feeAmount,
                FeesSymbol = feeSymbol,
                FeesPrice = 1,
                Date = transactionDate,
            },
        ];

        var (_, _, fiatFees) = _cryptoService.ProcessTransactions(transactions);

        fiatFees.Should().BeEquivalentTo(
        [
            new CryptoFiatFee()
            {
                Date = transactions.ElementAt(1).Date,
                FeesInEuros = expectedEuroAmount,
            },
        ]);
    }
}