using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services;

namespace RevoProfit.Test.Revolut;

public class RevolutServiceTest
{
    private RevolutService _revolutService = null!;
    private int _incrementHours;

    [SetUp]
    public void Setup()
    {
        _revolutService = new RevolutService(new CryptoService());
        _incrementHours = 0;
    }

    private RevolutTransaction Transaction() => new()
    {
        CompletedDate = DateTime.Now.AddDays(-10).AddHours(_incrementHours++),
        Description = string.Empty,
        Amount = 1,
        Currency = "BTC",
        FiatAmount = 10,
        FiatAmountIncludingFees = 11,
        Fee = 1,
        BaseCurrency = "EUR",
    };

    [Test]
    public void Test_when_migration_to_another_region_should_ignore()
    {
        // Arrange
        var transactions = new[]
        {
            Transaction() with { Description = "Balance migration to another region or legal entity" }
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Test_when_amount_is_positive_and_no_matching_transaction_at_same_time_should_return_a_depot()
    {
        // EXCHANGE,Current,2021-03-12 12:07:49,2021-03-12 12:07:49,Exchanged to BCH,0.00917915,BCH,4.14,4.2,0.06,EUR,COMPLETED,0.48372838
        // Arrange
        var transactions = new[]
        {
            new RevolutTransaction
            {
                CompletedDate = new DateTime(2022, 02, 05, 12, 30, 50),
                Description = string.Empty,
                Amount = 10,
                Currency = "BTC",
                FiatAmount = 100,
                FiatAmountIncludingFees = 110,
                Fee = 10,
                BaseCurrency = "EUR",
            }
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Depot,
                Date = new DateTime(2022, 02, 05, 12, 30, 50),
                MontantRecu = 10,
                MonnaieOuJetonRecu = "BTC",
                PrixDuJetonDuMontantRecu = 10,
                MontantEnvoye = 0,
                MonnaieOuJetonEnvoye = null,
                PrixDuJetonDuMontantEnvoye = 0,
                Frais = 10,
                MonnaieOuJetonDesFrais = "EUR",
                PrixDuJetonDesFrais = 1
            }
        });
    }

    [Test]
    public void Test_when_amount_is_negative_and_no_matching_transaction_at_same_time_should_return_a_retrait()
    {
        // EXCHANGE,Current,2021-02-26 10:45:25,2021-02-26 10:45:25,Exchanged to USD,-0.33413706,BCH,-133.89,-131.91,1.98,EUR,COMPLETED,0.34827498
        // Arrange
        var transactions = new[]
        {
            new RevolutTransaction
            {
                CompletedDate = new DateTime(2022, 02, 05, 12, 30, 50),
                Description = string.Empty,
                Amount = -11,
                Currency = "BTC",
                FiatAmount = -110,
                FiatAmountIncludingFees = -100,
                Fee = 10,
                BaseCurrency = "EUR",
            }
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Retrait,
                Date = new DateTime(2022, 02, 05, 12, 30, 50),
                MontantRecu = 0,
                MonnaieOuJetonRecu = null,
                PrixDuJetonDuMontantRecu = 0,
                MontantEnvoye = 11,
                MonnaieOuJetonEnvoye = "BTC",
                PrixDuJetonDuMontantEnvoye = 10,
                Frais = 10,
                MonnaieOuJetonDesFrais = "EUR",
                PrixDuJetonDesFrais = 1
            }
        });
    }

    [Test]
    public void Test_when_amount_positive_and_match_another_transaction_should_return_an_exchange_that_come_from_the_combination_of_both_transactions()
    {
        // Arrange
        // EXCHANGE,Current,2022-05-06 10:26:48,2022-05-06 10:26:48,Exchanged to ETH,-50.21745342,MATIC,-49.88,-49.13,0.74,EUR,COMPLETED,0
        // EXCHANGE,Current,2022-05-06 10:26:48,2022-05-06 10:26:48,Exchanged to ETH,0.01910747,ETH,49.16,49.91,0.75,EUR,COMPLETED,1.58871391
        var dateTimeOfTransfer = new DateTime(2022, 05, 01, 12, 12, 30);
        var transactions = new[]
        {
            Transaction() with
            {
                CompletedDate = dateTimeOfTransfer,
                Currency = "BTC",
                Amount = new decimal(-1.1),
                FiatAmount = -100,
                FiatAmountIncludingFees = -110,
                Fee = 10,
            },
            Transaction() with
            {
                CompletedDate = dateTimeOfTransfer, 
                Currency = "ETH",
                Amount = 9,
                FiatAmount = 90,
                FiatAmountIncludingFees = 100,
                Fee = 10,
            },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Echange,
                Date = dateTimeOfTransfer,

                MontantRecu = 9,
                MonnaieOuJetonRecu = "ETH",
                PrixDuJetonDuMontantRecu = 10,

                MontantEnvoye = 1.1,
                MonnaieOuJetonEnvoye = "BTC",
                PrixDuJetonDuMontantEnvoye = 100,
                
                Frais = 2,
                MonnaieOuJetonDesFrais = "ETH",
                PrixDuJetonDesFrais = 10,
            }
        });
    }
}