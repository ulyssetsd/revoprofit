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
        _revolutService = new RevolutService(new CryptoService(new CryptoTransactionFluentValidator(), new EuropeanCentralBankExchangeRateProvider()));
        _incrementHours = 0;
    }

    #region ConvertToCryptoTransactions

    private RevolutTransaction Transaction() => new()
    {
        CompletedDate = DateTime.Now.AddDays(-10).AddHours(_incrementHours++),
        Description = string.Empty,
        Amount = 1,
        Currency = "BTC",
        FiatAmount = 10,
        FiatAmountIncludingFees = 11,
        FiatFees = 1,
        BaseCurrency = "EUR",
    };

    [Test]
    public void Convert_when_migration_to_another_region_should_ignore()
    {
        // Arrange
        var transactions = new[]
        {
            Transaction() with { Description = "Balance migration to another region or legal entity" },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Convert_when_closing_transaction_should_ignore()
    {
        // Arrange
        var transactions = new[]
        {
            Transaction() with
            {
                Description = "Closing transaction",
                Amount = 0,
            },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Convert_when_three_depot_transactions_happened_on_the_same_time_should_ignore_work_fine()
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
        results.Should().HaveCount(3).And.AllSatisfy(transaction => transaction.Type.Should().Be(CryptoTransactionType.Buy));
    }

    [Test]
    public void Convert_when_amount_is_positive_and_no_matching_transaction_at_same_time_should_return_a_depot()
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
                FiatFees = 10,
                BaseCurrency = "EUR",
            },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Buy,
                Date = new DateTime(2022, 02, 05, 12, 30, 50),
                BuyAmount = 10,
                BuySymbol = "BTC",
                BuyPrice = 10,
                SellAmount = 0m,
                SellPrice = 0,
                FeesAmount = 10,
                FeesSymbol = "EUR",
                FeesPrice = 1,
                SellSymbol = string.Empty,
            },
        });
    }

    [Test]
    public void Convert_when_amount_is_negative_and_no_matching_transaction_at_same_time_should_return_a_retrait()
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
                FiatFees = 10,
                BaseCurrency = "EUR",
            },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.Sell,
                Date = new DateTime(2022, 02, 05, 12, 30, 50),
                BuyAmount = 0,
                BuyPrice = 0,
                SellAmount = 11m,
                SellSymbol = "BTC",
                SellPrice = 10,
                FeesAmount = 10,
                FeesSymbol = "EUR",
                FeesPrice = 1,
                BuySymbol = string.Empty,
            },
        });
    }

    [Test]
    public void Convert_when_amount_positive_and_match_another_transaction_should_return_an_exchange_that_come_from_the_combination_of_both_transactions()
    {
        // Arrange
        // EXCHANGE,Current,2022-05-06 10:26:48,2022-05-06 10:26:48,Exchanged to ETH,-50.21745342,MATIC,-49.88,-49.13,0.74,EUR,COMPLETED,0
        // EXCHANGE,Current,2022-05-06 10:26:48,2022-05-06 10:26:48,Exchanged to ETH,0.01910747,ETH,49.16,49.91,0.75,EUR,COMPLETED,1.58871391
        var dateTimeOfTransfer = new DateTime(2022, 05, 01, 12, 12, 30);
        var transactions = new[]
        {
            new RevolutTransaction
            {
                CompletedDate = dateTimeOfTransfer,
                Currency = "BTC",
                Amount = -1.1m,
                FiatAmount = -110,
                FiatAmountIncludingFees = -100,
                FiatFees = 10,
                Description = string.Empty,
                BaseCurrency = "EUR",
            },
            new RevolutTransaction
            {
                CompletedDate = dateTimeOfTransfer, 
                Currency = "ETH",
                Amount = 9,
                FiatAmount = 90,
                FiatAmountIncludingFees = 100,
                FiatFees = 10,
                Description = string.Empty,
                BaseCurrency = "EUR",
            },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeEquivalentTo(new CryptoTransaction[]
        {
            new()
            {
                Type = CryptoTransactionType.FeesOnly,
                Date = dateTimeOfTransfer,

                BuyAmount = 0,
                BuySymbol = string.Empty,
                BuyPrice = 0,

                SellAmount = 0,
                SellSymbol = string.Empty,
                SellPrice = 0,

                FeesAmount = 0.1m,
                FeesSymbol = "BTC",
                FeesPrice = 100,
            },
            new()
            {
                Type = CryptoTransactionType.Exchange,
                Date = dateTimeOfTransfer,

                BuyAmount = 9,
                BuySymbol = "ETH",
                BuyPrice = 10,

                SellAmount = 1.1m,
                SellSymbol = "BTC",
                SellPrice = 100,
                
                FeesAmount = 1,
                FeesSymbol = "ETH",
                FeesPrice = 10,
            },
        });
    }

    [Test]
    public void Convert_when_multiple_currency_transactions_are_sort_first_by_currrency_then_date_time_should_return_transactions_in_datetime_order_first()
    {
        // Arrange
        var transactions = new[]
        {
            Transaction() with
            {
                CompletedDate = DateTime.Now,
                Currency = "BTC",
            },
            Transaction() with
            {
                CompletedDate = DateTime.Now.AddDays(-1),
                Currency = "ETH",
            },
        };

        // Act
        var results = _revolutService.ConvertToCryptoTransactions(transactions);

        // Assert
        results.Should().BeInAscendingOrder(transaction => transaction.Date);
    }

    #endregion

    #region ProcessTransactions

    private RevolutTransaction Tran(string currency, decimal amount, decimal price, bool sameDateForNextTran = false) => new()
    {
        CompletedDate = new DateTime(2018, 06, 12, 14, 16, 32).AddHours(sameDateForNextTran ? _incrementHours : _incrementHours++),
        Description = "Exchanged",
        Amount = amount,
        Currency = currency,
        FiatAmount = price,
        FiatAmountIncludingFees = price,
        FiatFees = 0,
        BaseCurrency = "EUR",
    };

    [Test]
    public void Process_when_buy_and_sell_without_gains_should_have_retrait_without_gains()
    {
        // Arrange
        var transactions = new[]
        {
            Tran("BTC", 0.02m, 100),
            Tran("BTC", -0.01m, -50),
        };

        // Act
        var (assets, retraits, _) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEquivalentTo(new[]
        {
            new CryptoSell
            {
                Date = transactions[1].CompletedDate,
                Symbol = "BTC",
                Amount = 0.01m,
                AmountInEuros = 50,
                GainsInEuros = 0,
                Price = 5000,
            },
        });
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 50,
                Amount = 0.01m,
            },
        });
    }

    [Test]
    public void Process_when_buy_and_sell_with_gains_should_have_retrait_with_gains()
    {
        // Arrange
        var transactions = new[]
        {
            Tran("BTC", 0.02m, 100),
            Tran("BTC", -0.01m, -60),
        };

        // Act
        var (assets, retraits, _) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEquivalentTo(new[]
        {
            new CryptoSell
            {
                Date = transactions[1].CompletedDate,
                Symbol = "BTC",
                Amount = 0.01m,
                AmountInEuros = 60,
                GainsInEuros = 10,
                Price = 6000,
            },
        });
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 50,
                Amount = 0.01m,
            },
        });
    }

    [Test]
    public void Process_when_buy_and_exchange_should_have_euro_amount_equivalent_to_portion_of_amount_from_source_currency()
    {
        // Arrange
        var transactions = new[]
        {
            Tran("BTC", 0.02m, 100),
            Tran("BTC", -0.015m, -60, sameDateForNextTran: true),
            Tran("ETH", 0.1m, 60),
        };

        // Act
        var (assets, retraits, _) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEmpty();
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 25,
                Amount = 0.005m,
            },
            new CryptoAsset
            {
                Symbol = "ETH",
                AmountInEuros = 75,
                Amount = 0.1m,
            },
        });
    }

    [Test]
    public void Process_when_buy_exchange_and_sell_with_gains_should_have_gains_equivalent_of_portion_of_source_crypto_exchanged()
    {
        // Arrange
        var transactions = new[]
        {
            Tran("BTC", 0.02m, 100),
            Tran("BTC", -0.015m, -10, sameDateForNextTran: true),
            Tran("ETH", 0.1m, 10),
            Tran("ETH", -0.1m, -20),
        };

        // Act
        var (assets, retraits, _) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEquivalentTo(new[]
        {
            new CryptoSell
            {
                Date = transactions[3].CompletedDate,
                Symbol = "ETH",
                Amount = 0.1m,
                AmountInEuros = 20,
                GainsInEuros = -55,
                Price = 200,
            },
        });
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 25,
                Amount = 0.005m,
            },
            new CryptoAsset
            {
                Symbol = "ETH",
                AmountInEuros = 0,
                Amount = 0,
            },
        });
    }

    [Test]
    public void Process_when_buy_and_sell_all_then_buy_again_should_not_try_to_divide_by_zero_and_throw()
    {
        // Arrange
        var transactions = new[]
        {
            Tran("BTC", 0.02m, 100),
            Tran("BTC", -0.02m, -100),
            Tran("BTC", 0.1m, 1000),
        };

        // Act
        var (assets, retraits, _) = _revolutService.ProcessTransactions(transactions);

        // Assert
        retraits.Should().BeEquivalentTo(new[]
        {
            new CryptoSell
            {
                Date = transactions[1].CompletedDate,
                Symbol = "BTC",
                Amount = 0.02m,
                AmountInEuros = 100,
                GainsInEuros = 0,
                Price = 5000,
            },
        });
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 1000,
                Amount = 0.1m,
            },
        });
    }

    [Test]
    public void Process_when_exchange_with_fees_should_store_fees_infos_on_both_currencies()
    {
        var transactions = new[]
        {
            Tran("BTC", 1, 1),
            new RevolutTransaction
            {
                CompletedDate = new DateTime(2023, 02, 25, 01, 14, 30),
                Description = string.Empty,
                Amount = -1m,
                Currency = "BTC",
                FiatAmount = -1m,
                FiatAmountIncludingFees = -0.9m,
                FiatFees = 0.1m,
                BaseCurrency = "EUR",
            },
            new RevolutTransaction
            {
                CompletedDate = new DateTime(2023, 02, 25, 01, 14, 30),
                Description = string.Empty,
                Amount = 0.8m,
                Currency = "ETH",
                FiatAmount = 0.8m,
                FiatAmountIncludingFees = 0.9m,
                FiatFees = 0.1m,
                BaseCurrency = "EUR",
            },
        };

        // Act
        var (assets, retraits, fiatFees) = _revolutService.ProcessTransactions(transactions);

        // Assert
        fiatFees.Should().BeEmpty();
        retraits.Should().BeEmpty();
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 0m,
                Amount = 0m,
                Fees = 0.1m,
            },
            new CryptoAsset
            {
                Symbol = "ETH",
                AmountInEuros = 1m,
                Amount = 0.8m,
                Fees = 0.1m,
            },
        });
    }

    [Test]
    public void Process_when_fees_for_depot_in_euros_should_be_reported_correctly()
    {
        var transactions = new[]
        {
            new RevolutTransaction
            {
                CompletedDate = new DateTime(2023, 02, 25, 01, 14, 15),
                Description = string.Empty,
                Amount = 1m,
                Currency = "BTC",
                FiatAmount = 100,
                FiatAmountIncludingFees = 110,
                FiatFees = 10,
                BaseCurrency = "EUR",
            },
        };

        // Act
        var (assets, retraits, fiatFees) = _revolutService.ProcessTransactions(transactions);

        // Assert
        fiatFees.Should().BeEquivalentTo(new[]
        {
            new CryptoFiatFee
            {
                Date = new DateTime(2023, 02, 25, 01, 14, 15),
                FeesInEuros = 10,
            },
        });
        retraits.Should().BeEmpty();
        assets.Should().BeEquivalentTo(new[]
        {
            new CryptoAsset
            {
                Symbol = "BTC",
                AmountInEuros = 100,
                Amount = 1,
                Fees = 0,
            },
        });
    }

    #endregion
}