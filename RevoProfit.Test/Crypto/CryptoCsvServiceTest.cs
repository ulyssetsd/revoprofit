using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services;

namespace RevoProfit.Test.Crypto
{
    internal class CryptoCsvServiceTest
    {
        private const string Bitcoin = "BTC";
        private CryptoCsvService _cryptoCsvService;

        [SetUp]
        public void Setup()
        {
            _cryptoCsvService = new CryptoCsvService();
        }

        [Test]
        public async Task ReadCsv_WhenAValidContentIsGiven_ShouldReturnAMappedListOfTheContent()
        {
            // Arrange
            const string content = @"
Type,Date,Montant reçu,Monnaie ou jeton reçu,Montant envoyé,Monnaie ou jeton envoyé,Frais,Monnaie ou jeton des frais,Exchange / Plateforme,Description,Label,Prix du jeton du montant envoyé,Prix du jeton du montant recu,Prix du jeton des frais
Dépôt,12/06/2018 12:16:32,""0,01713112"",BTC,,,,,Revolut,Exchanged to SOL,,,""5708,036473"",
Retrait,19/08/2018 20:43:55,,,""0,008196"",BTC,,,Revolut,Exchanged to SOL,Paiement,""5504,071"",,
Échange,05/04/2021 23:24:05,""0,00959825"",BTC,""0,6"",BCH,""0,000144"",BTC,Revolut,Exchanged to SOL,,""776,0666485"",""46613,99539"",""46613,99539""
";
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var cryptoTransactions = (await _cryptoCsvService.ReadCsv(memoryStream)).ToArray();

            // Assert
            cryptoTransactions.Should().HaveCount(3);
            cryptoTransactions[0].Should().BeEquivalentTo(new CryptoTransaction
            {
                Type = CryptoTransactionType.Depot,
                Date = new DateTime(2018, 06, 12, 12, 16, 32),
                MontantRecu = 0.01713112,
                MonnaieOuJetonRecu = Bitcoin,
                ExchangePlateforme = "Revolut",
                Description = "Exchanged to SOL",
                PrixDuJetonDuMontantRecu = 5708.036473,

                Label = string.Empty,
                MonnaieOuJetonEnvoye = string.Empty,
                MonnaieOuJetonDesFrais = string.Empty,
            });
            cryptoTransactions[1].Should().BeEquivalentTo(new CryptoTransaction
            {
                Type = CryptoTransactionType.Retrait,
                Date = new DateTime(2018, 08, 19, 20, 43, 55),
                MontantEnvoye = 0.008196,
                MonnaieOuJetonEnvoye = Bitcoin,
                ExchangePlateforme = "Revolut",
                Description = "Exchanged to SOL",
                Label = "Paiement",
                PrixDuJetonDuMontantEnvoye = 5504.071,

                MonnaieOuJetonDesFrais = string.Empty,
                MonnaieOuJetonRecu = string.Empty,
            });
            cryptoTransactions[2].Should().BeEquivalentTo(new CryptoTransaction
            {
                Type = CryptoTransactionType.Echange,
                Date = new DateTime(2021, 04, 05, 23, 24, 05),
                MontantRecu = 0.00959825,
                MonnaieOuJetonRecu = Bitcoin,
                MontantEnvoye = 0.6,
                MonnaieOuJetonEnvoye = "BCH",
                Frais = 0.000144,
                MonnaieOuJetonDesFrais = Bitcoin,
                ExchangePlateforme = "Revolut",
                Description = "Exchanged to SOL",
                PrixDuJetonDuMontantEnvoye = 776.0666485,
                PrixDuJetonDuMontantRecu = 46613.99539,
                PrixDuJetonDesFrais = 46613.99539,

                Label = string.Empty,
            });
        }
    }
}
