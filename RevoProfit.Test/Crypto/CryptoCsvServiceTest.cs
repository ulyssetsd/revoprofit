using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services;

namespace RevoProfit.Test.Crypto;

internal class CryptoCsvServiceTest
{
    private CryptoCsvService _cryptoCsvService = null!;

    [SetUp]
    public void Setup()
    {
        _cryptoCsvService = new CryptoCsvService(new CryptoTransactionMapper());
    }

    [Test]
    public async Task ReadCsv_WhenAValidContentIsGiven_ShouldReturnAMappedListOfTheContent()
    {
        // Arrange
        var content = new StringBuilder()
            .AppendLine("Type,Date,Montant reçu,Monnaie ou jeton reçu,Montant envoyé,Monnaie ou jeton envoyé,Frais,Monnaie ou jeton des frais,Exchange / Plateforme,Description,Label,Prix du jeton du montant envoyé,Prix du jeton du montant recu,Prix du jeton des frais")
            .AppendLine("Dépôt,12/06/2018 12:16:32,\"0,01713112\",BTC,,,,,Revolut,Exchanged to SOL,,,\"5708,036473\",")
            .AppendLine("Retrait,19/08/2018 20:43:55,,,\"0,008196\",BTC,,,Revolut,Exchanged to SOL,Paiement,\"5504,071\",,")
            .AppendLine("Échange,05/04/2021 23:24:05,\"0,00959825\",BTC,\"0,6\",BCH,\"0,000144\",BTC,Revolut,Exchanged to SOL,,\"776,0666485\",\"46613,99539\",\"46613,99539\"")
            .ToString();
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var cryptoTransactions = (await _cryptoCsvService.ReadCsv(memoryStream)).ToArray();

        // Assert
        cryptoTransactions.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Buy,
                Date = new DateTime(2018, 06, 12, 12, 16, 32),
                BuyAmount = 0.01713112m,
                BuySymbol = "BTC",
                BuyPrice = 5708.036473m,

                SellSymbol = string.Empty,
                FeesSymbol = string.Empty,
                SellAmount = 0,
                SellPrice = 0,
                FeesAmount = 0,
                FeesPrice = 0,
            },
            new()
            {
                Type = CryptoTransactionType.Sell,
                Date = new DateTime(2018, 08, 19, 20, 43, 55),
                SellAmount = 0.008196m,
                SellSymbol = "BTC",
                SellPrice = 5504.071m,

                FeesSymbol = string.Empty,
                BuySymbol = string.Empty,
                BuyAmount = 0,
                BuyPrice = 0,
                FeesAmount = 0,
                FeesPrice = 0,
            },
            new()
            {
                Type = CryptoTransactionType.Exchange,
                Date = new DateTime(2021, 04, 05, 23, 24, 05),
                BuyAmount = 0.00959825m,
                BuySymbol = "BTC",
                SellAmount = 0.6m,
                SellSymbol = "BCH",
                FeesAmount = 0.000144m,
                FeesSymbol = "BTC",
                SellPrice = 776.0666485m,
                BuyPrice = 46613.99539m,
                FeesPrice = 46613.99539m,
            },
        });
    }

    [Test]
    public async Task ReadCsv_when_content_is_a_2022_waltio_export_whould_return_a_mapped_list()
    {
        // Arrange
        var content = new StringBuilder()
            .AppendLine("Type,Date,Fuseau horaire,Montant reçu,Monnaie ou jeton reçu,Montant envoyé,Monnaie ou jeton envoyé,Frais,Monnaie ou jeton des frais,Exchange / Plateforme,Description,Label,Prix du jeton du montant envoyé,Prix du jeton du montant recu,Prix du jeton des frais,Adresse,Transaction hash,ID Externe")
            .AppendLine("Dépôt,21/06/2021 07:28:57,GMT,1.518212,ADA,,,0.027005,ADA,Revolut - account-statement.csv,Exchanged to SOL,,,1.086911184,1.086911184,,,")
            .AppendLine("Retrait,26/02/2021 10:45:50,GMT,,,0.97592,LTC,0.014622,LTC,Revolut - account-statement.csv,Exchanged to SOL,Paiement,143.99,,143.99,,,")
            .AppendLine("Échange,19/04/2021 23:46:24,GMT,0.00047971,BTC,0.03,BCH,0.000007,BTC,Revolut - account-statement.csv,Exchanged to SOL,,776.0666485,46613.99539,46613.99539,,,")
            .ToString();
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var cryptoTransactions = (await _cryptoCsvService.ReadCsv(memoryStream)).ToArray();

        // Assert
        cryptoTransactions.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Buy,
                Date = new DateTime(2021, 06, 21, 07, 28, 57),
                BuyAmount = 1.518212m,
                BuySymbol = "ADA",
                SellAmount = 0m,
                SellSymbol = string.Empty,
                FeesAmount = 0.027005m,
                FeesSymbol = "ADA",
                SellPrice = 0m,
                BuyPrice = 1.086911184m,
                FeesPrice = 1.086911184m,
            },
            new()
            {
                Type = CryptoTransactionType.Sell,
                Date = new DateTime(2021, 02, 26, 10, 45, 50),
                BuyAmount = 0m,
                BuySymbol = string.Empty,
                SellAmount = 0.97592m,
                SellSymbol = "LTC",
                FeesAmount = 0.014622m,
                FeesSymbol = "LTC",
                SellPrice = 143.99m,
                BuyPrice = 0m,
                FeesPrice = 143.99m,
            },
            new()
            {
                Type = CryptoTransactionType.Exchange,
                Date = new DateTime(2021, 04, 19, 23, 46, 24),
                BuyAmount = 0.00047971m,
                BuySymbol = "BTC",
                SellAmount = 0.03m,
                SellSymbol = "BCH",
                FeesAmount = 0.000007m,
                FeesSymbol = "BTC",
                SellPrice = 776.0666485m,
                BuyPrice = 46613.99539m,
                FeesPrice = 46613.99539m,
            },
        });
    }
}