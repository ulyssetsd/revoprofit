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
    private const string bitcoin = "BTC";
    private const string ethereum = "ETH";

    [SetUp]
    public void Setup()
    {
        _cryptoService = new CryptoService();
        _dateIncrement = 0;
    }

    private bool Bitcoin(CryptoSell cryptoSell) => cryptoSell.Symbol == bitcoin;
    private bool Bitcoin(CryptoAsset cryptoAsset) => cryptoAsset.Symbol == bitcoin;
    private bool Ethereum(CryptoSell cryptoSell) => cryptoSell.Symbol == ethereum;
    private bool Ethereum(CryptoAsset cryptoAsset) => cryptoAsset.Symbol == ethereum;

    private CryptoTransaction Ethereum(CryptoTransactionType cryptoTransactionType, decimal prix, decimal quantite = 1, int yearIncrement = 0, decimal prixBitcoin = 1) =>
        CryptoTransaction(cryptoTransactionType, prix, quantite, yearIncrement, prixBitcoin, ethereum, bitcoin);

    private CryptoTransaction Bitcoin(CryptoTransactionType cryptoTransactionType, decimal prix, decimal quantite = 1, int yearIncrement = 0, decimal prixEthereum = 1) =>
        CryptoTransaction(cryptoTransactionType, prix, quantite, yearIncrement, prixEthereum, bitcoin, ethereum);

    private CryptoTransaction CryptoTransaction(CryptoTransactionType cryptoTransactionType, decimal prix, decimal quantite = 1, int yearIncrement = 0, decimal prixDestination = 1, string source = bitcoin, string destination = ethereum)
    {
        var date = DateTime.Today.AddYears(yearIncrement).AddDays(++_dateIncrement);

        return cryptoTransactionType switch
        {
            CryptoTransactionType.Buy => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                BuySymbol = source,
                BuyPrice = prix,
                BuyAmount = quantite,
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
                SellPrice = prix,
                SellAmount = quantite,
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
                SellPrice = prix,
                SellAmount = quantite,
                BuySymbol = destination,
                BuyPrice = prixDestination,
                BuyAmount = quantite * prix / prixDestination,
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
            Bitcoin(CryptoTransactionType.Buy, prix: 100, quantite: 1),
            Bitcoin(CryptoTransactionType.Sell, prix: 200, quantite: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).Should().BeEquivalentTo(new CryptoSell
        {
            Amount = 0.5m,
            AmountInEuros = 100,
            GainsInEuros = 50,
            Symbol = bitcoin,
            Price = 200,
            Date = transactions[1].Date,
        }, opt => opt.Excluding(x => x.Date));

        cryptoAssets.First(Bitcoin).Should().BeEquivalentTo(new CryptoAsset
        {
            Symbol = bitcoin,
            Amount = 0.5m,
            AmountInEuros = 50,
        });
    }

    [Test]
    public void TestCalculDesGainsAvecEchangeEtPrixSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Buy, prix: 100),
            Bitcoin(CryptoTransactionType.Exchange, prix: 100, quantite: .5m, prixEthereum: 100),
            Bitcoin(CryptoTransactionType.Sell, prix: 200, quantite: .5m),
            Ethereum(CryptoTransactionType.Sell, prix: 100, quantite: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).GainsInEuros.Should().Be(50);
        retraits.First(Ethereum).GainsInEuros.Should().Be(0);
        cryptoAssets.First(Bitcoin).AmountInEuros.Should().Be(0);
        cryptoAssets.First(Bitcoin).Amount.Should().Be(0);
        cryptoAssets.First(Ethereum).AmountInEuros.Should().Be(0);
        cryptoAssets.First(Ethereum).Amount.Should().Be(0);
    }

    [Test]
    public void TestCalculDesGainsAvecEchangeEtPrixNonSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Buy, prix: 100),
            Bitcoin(CryptoTransactionType.Exchange, prix: 200, quantite: .5m, prixEthereum: 100),
            Ethereum(CryptoTransactionType.Sell, prix: 200, quantite: 1),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.First(Ethereum).GainsInEuros.Should().Be(150);
        cryptoAssets.First(Bitcoin).Amount.Should().Be(0.5m);
        cryptoAssets.First(Bitcoin).AmountInEuros.Should().Be(50);
    }

    [Test]
    public void TestCalculDesGainsAvecPlusieursRetraitsEtEchangesEtPrixNonSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Buy, prix: 100),
            Bitcoin(CryptoTransactionType.Exchange, prix: 200, quantite: .5m, prixEthereum: 100),
            Ethereum(CryptoTransactionType.Sell, prix: 200, quantite: 1),
            Bitcoin(CryptoTransactionType.Sell, prix: 300, quantite: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).GainsInEuros.Should().Be(100);
        retraits.First(Ethereum).GainsInEuros.Should().Be(150);
        cryptoAssets.First(Bitcoin).AmountInEuros.Should().Be(0);
    }

    [Test]
    public void TestCalculDesGainsAvecPlusieursRetraitsEtEchangesEtPrixNonSimilaires2()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Buy, prix: 100),
            Bitcoin(CryptoTransactionType.Exchange, prix: 200, quantite: .5m, prixEthereum: 100),
            Ethereum(CryptoTransactionType.Buy, prix: 200, quantite: 1),
            Ethereum(CryptoTransactionType.Sell, prix: 300, quantite: 2),
            Bitcoin(CryptoTransactionType.Sell, prix: 300, quantite: .5m),
        };

        var (cryptoAssets, retraits, _) = _cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).GainsInEuros.Should().Be(100);
        retraits.First(Ethereum).GainsInEuros.Should().Be(350);
        cryptoAssets.First(Bitcoin).AmountInEuros.Should().Be(0);
        cryptoAssets.First(Ethereum).AmountInEuros.Should().Be(0);
    }
}