using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private bool Bitcoin(Retrait retrait) => retrait.Jeton == bitcoin;
        private bool Bitcoin(CryptoAsset cryptoAsset) => cryptoAsset.Jeton == bitcoin;
        private bool Ethereum(Retrait retrait) => retrait.Jeton == ethereum;
        private bool Ethereum(CryptoAsset cryptoAsset) => cryptoAsset.Jeton == ethereum;

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
                    MonnaieOuJetonRecu = source,
                    PrixDuJetonDuMontantReçu = prix,
                    MontantRecu = quantité,
                },
                CryptoTransactionType.Retrait => new CryptoTransaction
                {
                    Date = date,
                    Type = cryptoTransactionType,
                    MonnaieOuJetonEnvoye = source,
                    PrixDuJetonDuMontantEnvoye = prix,
                    MontantEnvoye = quantité,
                },
                CryptoTransactionType.Échange => new CryptoTransaction
                {
                    Date = date,
                    Type = cryptoTransactionType,
                    MonnaieOuJetonEnvoye = source,
                    PrixDuJetonDuMontantEnvoye = prix,
                    MontantEnvoye = quantité,
                    MonnaieOuJetonRecu = destination,
                    PrixDuJetonDuMontantReçu = prixDestination,
                    MontantRecu = (quantité * prix) / prixDestination,
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

            retraits.First(Bitcoin).Should().BeEquivalentTo(new Retrait
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
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100),
                Bitcoin(CryptoTransactionType.Échange, prix: 100, quantité: .5, prixEthereum: 100),
                Bitcoin(CryptoTransactionType.Retrait, prix: 200, quantité: .5),
                Ethereum(CryptoTransactionType.Retrait, prix: 100, quantité: .5),
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
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100),
                Bitcoin(CryptoTransactionType.Échange, prix: 200, quantité: .5, prixEthereum: 100),
                Ethereum(CryptoTransactionType.Retrait, prix: 200, quantité: 1),
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
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100),
                Bitcoin(CryptoTransactionType.Échange, prix: 200, quantité: .5, prixEthereum: 100),
                Ethereum(CryptoTransactionType.Retrait, prix: 200, quantité: 1),
                Bitcoin(CryptoTransactionType.Retrait, prix: 300, quantité: .5),
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
                Bitcoin(CryptoTransactionType.Dépôt, prix: 100),
                Bitcoin(CryptoTransactionType.Échange, prix: 200, quantité: .5, prixEthereum: 100),
                Ethereum(CryptoTransactionType.Dépôt, prix: 200, quantité: 1),
                Ethereum(CryptoTransactionType.Retrait, prix: 300, quantité: 2),
                Bitcoin(CryptoTransactionType.Retrait, prix: 300, quantité: .5),
            };

            var (cryptoAssets, retraits) = cryptoService.ProcessTransactions(transactions);

            retraits.First(Bitcoin).GainsEnEuros.Should().BeApproximately(100, 0.1);
            retraits.First(Ethereum).GainsEnEuros.Should().BeApproximately(350, 0.1);
            cryptoAssets.First(Bitcoin).MontantEnEuros.Should().Be(0);
            cryptoAssets.First(Ethereum).MontantEnEuros.Should().Be(0);
        }

        [Test]
        public async Task ReadCsv_WhenAValidContentIsGiven_ShouldReturnAMappedListOfTheContent()
        {
            // Arrange
            const string content = @"
Type,Date,Montant reçu,Monnaie ou jeton reçu,Montant envoyé,Monnaie ou jeton envoyé,Frais,Monnaie ou jeton des frais,Exchange / Plateforme,Description,Label,Prix du jeton du montant envoyé,Prix du jeton du montant recu,Prix du jeton des frais
Dépôt,12/06/2018 14:16:32,""0,01713112"",BTC,,,,,Revolut,Exchanged to SOL,,,""5708,036473"",
Retrait,19/08/2018 20:43:55,,,""0,008196"",BTC,,,Revolut,Exchanged to SOL,Paiement,""5504,071"",,
";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var cryptoTransactions = (await cryptoService.ReadCsv(memoryStream)).ToArray();

            // Assert
            cryptoTransactions.Should().HaveCount(2);
            cryptoTransactions[0].Type.Should().Be(CryptoTransactionType.Dépôt);
            cryptoTransactions[0].MonnaieOuJetonRecu.Should().Be(bitcoin);
            cryptoTransactions[0].MontantRecu.Should().Be(0.01713112);
            cryptoTransactions[1].Type.Should().Be(CryptoTransactionType.Retrait);
            cryptoTransactions[1].MonnaieOuJetonEnvoye.Should().Be(bitcoin);
            cryptoTransactions[1].MontantEnvoye.Should().Be(0.008196);
        }
    }
}