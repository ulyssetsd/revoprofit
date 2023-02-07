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
    public void Test_when_closing_transaction_should_ignore()
    {
        // Arrange
        var transactions = new[]
        {
            Transaction() with
            {
                Description = "Closing transaction",
                Amount = 0,
            }
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Test_when_three_depot_transactions_happened_on_the_same_time_should_ignore_work_fine()
    {
        // Arrange
        var date = DateTime.Today;
        var transactions = new[]
        {
            Transaction() with { CompletedDate = date },
            Transaction() with { CompletedDate = date },
            Transaction() with { CompletedDate = date },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().HaveCount(3).And.AllSatisfy(transaction => transaction.Type.Should().Be(CryptoTransactionType.Depot));
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
                MontantEnvoye = 0m,
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
                PrixDuJetonDuMontantRecu = 0,
                MontantEnvoye = 11m,
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
                Amount = -1.1m,
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

                MontantEnvoye = 1.1m,
                MonnaieOuJetonEnvoye = "BTC",
                PrixDuJetonDuMontantEnvoye = 100,
                
                Frais = 2,
                MonnaieOuJetonDesFrais = "ETH",
                PrixDuJetonDesFrais = 10,
            }
        });
    }

    private RevolutTransaction Btc(decimal amount, decimal price) => new()
    {
        CompletedDate = new DateTime(2018, 06, 12, 14, 16, 32).AddHours(_incrementHours++),
        Description = "Exchanged",
        Amount = amount,
        Currency = "BTC",
        FiatAmount = price,
        FiatAmountIncludingFees = price,
        Fee = 0,
        BaseCurrency = "EUR",
    };

    [Test]
    public void Process_when_buy_and_sell_without_gains_should_have_retrait_without_gains()
    {
        // EXCHANGE,Current,2018-06-12 14:16:32,2018-06-12 14:16:32,Exchanged to BTC,0.01713112,BTC,100,100,0,EUR,COMPLETED,0.01713112
        // CARD_PAYMENT,Current,2018-07-19 15:52:15,2018-07-20 5:28:14,Hotel On Booking.com,-0.00893541,BTC,-56.53,-56.53,0,EUR,COMPLETED,0.00819571
        // Arrange
        var transactions = new[]
        {
            Btc(0.02m, 100),
            Btc(-0.01m, -50),
        };

        // Act
        var (assets, retraits) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEquivalentTo(new[]
        {
            new CryptoRetrait
            {
                Date = transactions[1].CompletedDate,
                Jeton = "BTC",
                Montant = 0.01m,
                MontantEnEuros = 50,
                GainsEnEuros = 0,
                PrixDuJeton = 5000,
            }
        });
    }

    [Test]
    public void Process_when_buy_and_sell_with_gains_should_have_retrait_with_gains()
    {
        // EXCHANGE,Current,2018-06-12 14:16:32,2018-06-12 14:16:32,Exchanged to BTC,0.01713112,BTC,100,100,0,EUR,COMPLETED,0.01713112
        // CARD_PAYMENT,Current,2018-07-19 15:52:15,2018-07-20 5:28:14,Hotel On Booking.com,-0.00893541,BTC,-56.53,-56.53,0,EUR,COMPLETED,0.00819571
        // Arrange
        var transactions = new[]
        {
            Btc(0.02m, 100),
            Btc(-0.01m, -60),
        };

        // Act
        var (assets, retraits) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEquivalentTo(new[]
        {
            new CryptoRetrait
            {
                Date = transactions[1].CompletedDate,
                Jeton = "BTC",
                Montant = 0.01m,
                MontantEnEuros = 60,
                GainsEnEuros = 10,
                PrixDuJeton = 6000,
            }
        });
    }
}