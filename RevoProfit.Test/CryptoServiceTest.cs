using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevoProfit.Test
{
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

        private class TransactionConfig
        {
            public CryptoTransactionType CryptoTransactionType { get; set; }
            public double Prix { get; set; }
            public double Quantité { get; set; } = 1;
            public int YearIncrement { get; set; } = 0;
            public double PrixDestination { get; set; } = 1;
        }

        private CryptoTransaction Ethereum(CryptoTransactionType cryptoTransactionType, double prix, double quantité = 1, int yearIncrement = 0, double prixBitcoin = 1) =>
            CryptoTransaction(cryptoTransactionType, prix, quantité, yearIncrement, prixBitcoin, ethereum, bitcoin);

        private CryptoTransaction Bitcoin(CryptoTransactionType cryptoTransactionType, double prix, double quantité = 1, int yearIncrement = 0, double prixEthereum = 1) =>
            CryptoTransaction(cryptoTransactionType, prix, quantité, yearIncrement, prixEthereum, bitcoin, ethereum);

        private CryptoTransaction CryptoTransaction(CryptoTransactionType cryptoTransactionType, double prix, double quantité = 1, int yearIncrement = 0, double prixDestination = 1, string source = bitcoin, string destination = ethereum)
        {
            DateTime date = DateTime.Today.AddYears(yearIncrement).AddDays(++_dateIncrement);

            return cryptoTransactionType switch
            {
                CryptoTransactionType.Dépôt => new CryptoTransaction
                {
                    Date = date,
                    Type = cryptoTransactionType,
                    MonnaieOuJetonReçu = source,
                    PrixDuJetonDuMontantReçu = prix,
                    MontantReçu = quantité,
                },
                CryptoTransactionType.Retrait => new CryptoTransaction
                {
                    Date = date,
                    Type = cryptoTransactionType,
                    MonnaieOuJetonEnvoyé = source,
                    PrixDuJetonDuMontantEnvoyé = prix,
                    MontantEnvoyé = quantité,
                },
                CryptoTransactionType.Échange => new CryptoTransaction
                {
                    Date = date,
                    Type = cryptoTransactionType,
                    MonnaieOuJetonEnvoyé = source,
                    PrixDuJetonDuMontantEnvoyé = prix,
                    MontantEnvoyé = quantité,
                    MonnaieOuJetonReçu = destination,
                    PrixDuJetonDuMontantReçu = prixDestination,
                    MontantReçu = (quantité * prix) / prixDestination,
                },
                _ => throw new NotImplementedException()
            };
        }

        [Test]
        public void TestCalculDesGainsSansEchange()
        {
            var transactions = new List<CryptoTransaction>
            {
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100, quantité: 1),
                Bitcoin(CryptoTransactionType.Retrait, prix: 200, quantité: .5),
            };

            var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

            retraits.First().Should().BeEquivalentTo(new Retrait
            {
                Montant = 0.5,
                MontantEnDollars = 100,
                Gains = 0.25,
                GainsEnDollars = 50,
                Jeton = bitcoin,
                PrixDuJetonDuMontant = 200,
            }, opt => opt.Excluding(x => x.Date));

            cryptoAssets.First().Should().BeEquivalentTo(new CryptoAsset
            {
                Jeton = bitcoin,
                Montant = 0.5,
                MontantEnDollars = 50,
            });
        }

        [Test]
        public void TestCalculDesGainsAvecEchangeEtPrixSimilaires()
        {
            var transactions = new List<CryptoTransaction>
            {
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100),
                Bitcoin(CryptoTransactionType.Échange, prix: 100, quantité: .5, prixEthereum: 100),
                Bitcoin(CryptoTransactionType.Retrait, prix: 200, quantité: .5),
                Ethereum(CryptoTransactionType.Retrait, prix: 100, quantité: .5),
            };

            var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

            retraits[0].GainsEnDollars.Should().Be(50);
            retraits[1].GainsEnDollars.Should().Be(0);
            cryptoAssets[0].Should().BeEquivalentTo(new CryptoAsset
            {
                Jeton = bitcoin,
                Montant = 0,
                MontantEnDollars = 0,
            });
            cryptoAssets[1].Should().BeEquivalentTo(new CryptoAsset
            {
                Jeton = ethereum,
                Montant = 0,
                MontantEnDollars = 0,
            });
        }

        [Test]
        public void TestCalculDesGainsAvecEchangeEtPrixNonSimilaires()
        {
            var transactions = new List<CryptoTransaction>
            {
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100),
                Bitcoin(CryptoTransactionType.Échange, prix: 200, quantité: .5, prixEthereum: 100),
                Ethereum(CryptoTransactionType.Retrait, prix: 200, quantité: 1),
            };

            var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

            retraits[0].GainsEnDollars.Should().Be(150);
            cryptoAssets[0].Should().BeEquivalentTo(new CryptoAsset
            {
                Jeton = bitcoin,
                Montant = 0.5,
                MontantEnDollars = 50,
            });

            transactions.Add(Bitcoin(CryptoTransactionType.Retrait, prix: 300, quantité: .5));

            (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

            retraits[0].GainsEnDollars.Should().Be(150);
            retraits[1].GainsEnDollars.Should().BeApproximately(100, 0.1);
            cryptoAssets[0].Should().BeEquivalentTo(new CryptoAsset
            {
                Jeton = bitcoin,
                Montant = 0,
                MontantEnDollars = 0,
            });
        }
    }
}