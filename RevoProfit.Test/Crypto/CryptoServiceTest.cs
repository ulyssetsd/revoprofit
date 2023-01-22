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
    private CryptoService cryptoService;
    private int _dateIncrement;
    const string bitcoin = "BTC";
    const string ethereum = "ETH";

    [SetUp]
    public void Setup()
    {
        cryptoService = new CryptoService();
        _dateIncrement = 0;
    }

    private bool Bitcoin(CryptoRetrait cryptoRetrait) => cryptoRetrait.Jeton == bitcoin;
    private bool Bitcoin(CryptoAsset cryptoAsset) => cryptoAsset.Jeton == bitcoin;
    private bool Ethereum(CryptoRetrait cryptoRetrait) => cryptoRetrait.Jeton == ethereum;
    private bool Ethereum(CryptoAsset cryptoAsset) => cryptoAsset.Jeton == ethereum;

    private CryptoTransaction Ethereum(CryptoTransactionType cryptoTransactionType, double prix, double quantite = 1, int yearIncrement = 0, double prixBitcoin = 1) =>
        CryptoTransaction(cryptoTransactionType, prix, quantite, yearIncrement, prixBitcoin, ethereum, bitcoin);

    private CryptoTransaction Bitcoin(CryptoTransactionType cryptoTransactionType, double prix, double quantite = 1, int yearIncrement = 0, double prixEthereum = 1) =>
        CryptoTransaction(cryptoTransactionType, prix, quantite, yearIncrement, prixEthereum, bitcoin, ethereum);

    private CryptoTransaction CryptoTransaction(CryptoTransactionType cryptoTransactionType, double prix, double quantite = 1, int yearIncrement = 0, double prixDestination = 1, string source = bitcoin, string destination = ethereum)
    {
        DateTime date = DateTime.Today.AddYears(yearIncrement).AddDays(++_dateIncrement);

        return cryptoTransactionType switch
        {
            CryptoTransactionType.Depot => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                MonnaieOuJetonRecu = source,
                PrixDuJetonDuMontantRecu = prix,
                MontantRecu = quantite,
            },
            CryptoTransactionType.Retrait => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                MonnaieOuJetonEnvoye = source,
                PrixDuJetonDuMontantEnvoye = prix,
                MontantEnvoye = quantite,
            },
            CryptoTransactionType.Echange => new CryptoTransaction
            {
                Date = date,
                Type = cryptoTransactionType,
                MonnaieOuJetonEnvoye = source,
                PrixDuJetonDuMontantEnvoye = prix,
                MontantEnvoye = quantite,
                MonnaieOuJetonRecu = destination,
                PrixDuJetonDuMontantRecu = prixDestination,
                MontantRecu = quantite * prix / prixDestination,
            },
            _ => throw new NotImplementedException()
        };
    }

    [Test]
    public void TestCalculDesGainsSansEchange()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Depot, prix: 100, quantite: 1),
            Bitcoin(CryptoTransactionType.Retrait, prix: 200, quantite: .5),
        };

        var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).Should().BeEquivalentTo(new CryptoRetrait
        {
            Montant = 0.5,
            MontantEnEuros = 100,
            Gains = 0.25,
            GainsEnEuros = 50,
            Jeton = bitcoin,
            PrixDuJetonDuMontant = 200,
            Frais = 0,
            FraisEnEuros = 0,
            ValeurGlobale = 200,
            PrixAcquisition = 100,
        }, opt => opt.Excluding(x => x.Date));

        cryptoAssets.First(Bitcoin).Should().BeEquivalentTo(new CryptoAsset
        {
            Jeton = bitcoin,
            Montant = 0.5,
            MontantEnEuros = 50,
        });
    }

    [Test]
    public void TestCalculDesGainsAvecEchangeEtPrixSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Depot, prix: 100),
            Bitcoin(CryptoTransactionType.Echange, prix: 100, quantite: .5, prixEthereum: 100),
            Bitcoin(CryptoTransactionType.Retrait, prix: 200, quantite: .5),
            Ethereum(CryptoTransactionType.Retrait, prix: 100, quantite: .5),
        };

        var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).GainsEnEuros.Should().Be(50);
        retraits.First(Ethereum).GainsEnEuros.Should().Be(0);
        cryptoAssets.First(Bitcoin).MontantEnEuros.Should().Be(0);
        cryptoAssets.First(Bitcoin).Montant.Should().Be(0);
        cryptoAssets.First(Ethereum).MontantEnEuros.Should().Be(0);
        cryptoAssets.First(Ethereum).Montant.Should().Be(0);
    }

    [Test]
    public void TestCalculDesGainsAvecEchangeEtPrixNonSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Depot, prix: 100),
            Bitcoin(CryptoTransactionType.Echange, prix: 200, quantite: .5, prixEthereum: 100),
            Ethereum(CryptoTransactionType.Retrait, prix: 200, quantite: 1),
        };

        var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

        retraits.First(Ethereum).GainsEnEuros.Should().Be(150);
        cryptoAssets.First(Bitcoin).Montant.Should().Be(0.5);
        cryptoAssets.First(Bitcoin).MontantEnEuros.Should().Be(50);
    }

    [Test]
    public void TestCalculDesGainsAvecPlusieursRetraitsEtEchangesEtPrixNonSimilaires()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Depot, prix: 100),
            Bitcoin(CryptoTransactionType.Echange, prix: 200, quantite: .5, prixEthereum: 100),
            Ethereum(CryptoTransactionType.Retrait, prix: 200, quantite: 1),
            Bitcoin(CryptoTransactionType.Retrait, prix: 300, quantite: .5),
        };

        var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).GainsEnEuros.Should().BeApproximately(100, 0.1);
        retraits.First(Ethereum).GainsEnEuros.Should().Be(150);
        cryptoAssets.First(Bitcoin).MontantEnEuros.Should().Be(0);
    }

    [Test]
    public void TestCalculDesGainsAvecPlusieursRetraitsEtEchangesEtPrixNonSimilaires2()
    {
        var transactions = new List<CryptoTransaction>
        {
            Bitcoin(CryptoTransactionType.Depot, prix: 100),
            Bitcoin(CryptoTransactionType.Echange, prix: 200, quantite: .5, prixEthereum: 100),
            Ethereum(CryptoTransactionType.Depot, prix: 200, quantite: 1),
            Ethereum(CryptoTransactionType.Retrait, prix: 300, quantite: 2),
            Bitcoin(CryptoTransactionType.Retrait, prix: 300, quantite: .5),
        };

        var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

        retraits.First(Bitcoin).GainsEnEuros.Should().BeApproximately(100, 0.1);
        retraits.First(Ethereum).GainsEnEuros.Should().BeApproximately(350, 0.1);
        cryptoAssets.First(Bitcoin).MontantEnEuros.Should().Be(0);
        cryptoAssets.First(Ethereum).MontantEnEuros.Should().Be(0);
    }
}