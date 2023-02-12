using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services;

namespace RevoProfit.Test.Stock;

public class StockTransactionServiceTest
{
    private StockTransactionService _stockTransactionService = null!;
    private int _dateIncrement;

    [SetUp]
    public void Setup()
    {
        _stockTransactionService = new StockTransactionService();
        _dateIncrement = 0;
    }

    private StockTransaction Tesla(StockTransactionType stockTransactionType, decimal price = 1, decimal quantity = 1, int yearIncrement = 0, decimal fxRate = 1) => new()
    {
        Date = DateTime.Today.AddYears(yearIncrement).AddDays(++_dateIncrement),
        Ticker = "TSLA",
        Type = stockTransactionType,
        Quantity = quantity,
        PricePerShare = price,
        TotalAmount = price * quantity,
        FxRate = fxRate,
    };

    [Test]
    public void Process_when_buying_and_selling_everything_from_a_single_stock_should_get_the_gains_and_stock_as_zero_quantity()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 200),
            Tesla(StockTransactionType.Sell, 1000),
        };

        // Act
        var (reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.Should().SatisfyRespectively(report => report.SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
            {
                StockSellOrders = new[]
                {
                    new StockSellOrder
                    {
                        Date = stockTransactions[1].Date,
                        Ticker = "TSLA",
                        Amount = 1000,
                        Gains = 800,
                        Quantity = 1,
                        GainsInEuros = 800,
                    },
                },
                Gains = 800,
                GainsInEuro = 800,
            }));
        stocks.Should().BeEquivalentTo(new[]
        {
            new StockOwned
            {
                Ticker = "TSLA",
                Quantity = 0,
                ValueInserted = 0,
                AveragePrice = 0,
            },
        });
    }

    [Test]
    public void Process_when_buying_and_selling_should_return_reports_for_each_year()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, quantity: 2) with { Date = new DateTime(2022, 01, 01) },
            Tesla(StockTransactionType.Sell) with { Date = new DateTime(2022, 01, 01) },
            Tesla(StockTransactionType.Sell) with { Date = new DateTime(2023, 01, 01) },
        };

        // Act
        var (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.Should().HaveCount(2).And.SatisfyRespectively(
            report => report.Year.Should().Be(2022),
            report => report.Year.Should().Be(2023)
        );
    }

    [Test]
    public void Process_when_no_sell_transaction_report_should_return_an_empty_gains_in_report()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
        };

        // Act
        var (reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = Array.Empty<StockSellOrder>(),
            Gains = 0,
            GainsInEuro = 0,
        });
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 2,
            ValueInserted = 40,
            AveragePrice = 20,
        });
    }

    [Test]
    public void Process_when_two_buys_on_different_price_and_one_sell_should_return_a_gains_equivalent_to_the_average_stock_price()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
            Tesla(StockTransactionType.Sell, 30),
        };

        // Act
        var (reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new []
            {
                new StockSellOrder
                {
                    Date = stockTransactions[2].Date,
                    Ticker = "TSLA",
                    Amount = 30,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 10,
                },
            },
            Gains = 10,
            GainsInEuro = 10,
        });
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 20,
            AveragePrice = 20,
        });
    }

    [Test]
    public void Process_when_stock_was_sold_then_buy_again_asold_again_should_use_an_average_stock_price_relative_to_the_quantity()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
            Tesla(StockTransactionType.Sell, 30),
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Sell, 30),
        };

        // Act
        var (reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new []
            {
                new StockSellOrder
                {
                    Date = stockTransactions[2].Date,
                    Ticker = "TSLA",
                    Amount = 30,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 10,
                },
                new StockSellOrder
                {
                    Date = stockTransactions[4].Date,
                    Ticker = "TSLA",
                    Amount = 30,
                    Gains = 15,
                    Quantity = 1,
                    GainsInEuros = 15,
                },
            },
            Gains = 25,
            GainsInEuro = 25,
        });
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 15,
            AveragePrice = 15,
        });
    }

    [Test]
    public void Process_when_selling_below_price_should_handle_the_negative_gains_correctly()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
            Tesla(StockTransactionType.Sell, 30),
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Sell, 30),
            Tesla(StockTransactionType.Sell, 10),
        };

        // Act
        var (reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new[]
            {
                new StockSellOrder
                {
                    Date = stockTransactions[2].Date,
                    Ticker = "TSLA",
                    Amount = 30,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 10,
                },
                new StockSellOrder
                {
                    Date = stockTransactions[4].Date,
                    Ticker = "TSLA",
                    Amount = 30,
                    Gains = 15,
                    Quantity = 1,
                    GainsInEuros = 15,
                },
                new StockSellOrder
                {
                    Date = stockTransactions[5].Date,
                    Ticker = "TSLA",
                    Amount = 10,
                    Gains = -5,
                    Quantity = 1,
                    GainsInEuros = -5,
                },
            },
            Gains = 20, 
            GainsInEuro = 20,
        });
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 0,
            ValueInserted = 0,
            AveragePrice = 0,
        });
    }

    [Test]
    public void TestStockSplitCalculation()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
        };

        // Act
        var (_, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 10,
            AveragePrice = 10,
        });

        // Arrange
        stockTransactions.AddRange(new List<StockTransaction>
        {
            new()
            {
                Date = DateTime.Today.AddDays(++_dateIncrement),
                Ticker = "TSLA",
                Type = StockTransactionType.StockSplit,
                Quantity = 9,
                FxRate = 1,
                PricePerShare = 0,
                TotalAmount = 0,
            },
        });

        // Act
        (_, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 10,
            ValueInserted = 10,
            AveragePrice = 1,
        });

        stockTransactions.AddRange(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Sell, 1, 10),
        });

        // Act
        (var reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new[]
            {
                new StockSellOrder
                {
                    Date = stockTransactions[2].Date, 
                    Ticker = "TSLA", 
                    Amount = 10, 
                    Gains = 0, 
                    Quantity = 10,
                    GainsInEuros = 0,
                },
            },
            Gains = 0,
            GainsInEuro = 0,
        });
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 0,
            ValueInserted = 0,
            AveragePrice = 0,
        });
    }

    [Test]
    public void TestFxRateGainsCalculation()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
        };

        // Act
        var (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new[]
            {
                new StockSellOrder
                {
                    Date = stockTransactions[1].Date,
                    Ticker = "TSLA",
                    Amount = 20,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 5,
                },
            },
            Gains = 10,
            GainsInEuro = 5,
        });

        // Arrange
        stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
            Tesla(StockTransactionType.Sell, 20, fxRate: 1),
        };

        // Act
        (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new[]
            {
                new StockSellOrder
                {
                    Date = stockTransactions[1].Date,
                    Ticker = "TSLA",
                    Amount = 20,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 5,
                },
                new StockSellOrder
                {
                    Date = stockTransactions[2].Date,
                    Ticker = "TSLA",
                    Amount = 20,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 10,
                },
            },
            Gains = 20,
            GainsInEuro = 15,
        });

        // Arrange
        stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
            Tesla(StockTransactionType.Sell, 20, fxRate: 1),
            Tesla(StockTransactionType.Sell, 20, fxRate: 0.5m),
        };

        // Act
        (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().SellReport.Should().BeEquivalentTo(new StockSellAnnualReport
        {
            StockSellOrders = new[]
            {
                new StockSellOrder
                {
                    Date = stockTransactions[1].Date,
                    Ticker = "TSLA",
                    Amount = 20,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 5,
                },
                new StockSellOrder
                {
                    Date = stockTransactions[2].Date,
                    Ticker = "TSLA",
                    Amount = 20,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 10,
                },
                new StockSellOrder
                {
                    Date = stockTransactions[3].Date,
                    Ticker = "TSLA",
                    Amount = 20,
                    Gains = 10,
                    Quantity = 1,
                    GainsInEuros = 20,
                },
            },
            Gains = 30,
            GainsInEuro = 35,
        });
    }

    [Test]
    public void Process_when_custody_fee_cash_top_up_or_dividend_should_insert_those_into_annual_report()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            new()
            {
                Date = DateTime.Today,
                Type = StockTransactionType.CustodyFee,
                TotalAmount = 100,
                FxRate = 0.5m,
                Ticker = string.Empty,
                Quantity = 0,
                PricePerShare = 0,
            },
            new()
            {
                Date = DateTime.Today,
                Type = StockTransactionType.CashTopUp,
                TotalAmount = 100,
                FxRate = 2,
                Ticker = string.Empty,
                Quantity = 0,
                PricePerShare = 0,
            },
            new()
            {
                Date = DateTime.Today,
                Type = StockTransactionType.Dividend,
                Ticker = "TSLA",
                TotalAmount = 100,
                FxRate = 1,
                Quantity = 0,
                PricePerShare = 0,
            },
        };

        // Act
        var (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().Should().BeEquivalentTo(new StockAnnualReport
        {
            Year = DateTime.Today.Year,
            CustodyFee = 100,
            CustodyFeeInEuro = 200,
            CashTopUp = 100,
            CashWithdrawal = 0,
            CashTopUpInEuro = 50,
            CashWithdrawalInEuro = 0,
            Dividends = 100,
            DividendsInEuro = 100,
            SellReport = new StockSellAnnualReport
            {
                StockSellOrders = Array.Empty<StockSellOrder>(),
                Gains = 0,
                GainsInEuro = 0,
            },
        });
    }

    [Test]
    public void Test_cash_withdrawal_transaction_behaviour()
    {
        // Arrange
        var stockTransactions = new List<StockTransaction>
        {
            new()
            {
                Date = DateTime.Today.AddDays(-1),
                Ticker = string.Empty,
                Type = StockTransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 200,
                FxRate = 0.5m,
            },
            new()
            {
                Date = DateTime.Today.AddDays(0),
                Ticker = string.Empty,
                Type = StockTransactionType.CashWithdrawal,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 200,
                FxRate = 2,
            },
        };

        // Act
        var (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        // Assert
        reports.First().Should().BeEquivalentTo(new StockAnnualReport
        {
            Year = DateTime.Today.Year,
            Dividends = 0,
            CashTopUp = 200,
            CashWithdrawal = 200,
            CustodyFee = 0,
            DividendsInEuro = 0,
            CashTopUpInEuro = 400,
            CashWithdrawalInEuro = 100,
            CustodyFeeInEuro = 0,
            SellReport = new StockSellAnnualReport
            {
                StockSellOrders = Array.Empty<StockSellOrder>(),
                Gains = 0,
                GainsInEuro = 0,
            },
        });
    }
}