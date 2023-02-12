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
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 200),
            Tesla(StockTransactionType.Sell, 1000),
        };

        var (reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        reports.First().Gains.Should().Be(800);
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 0,
            ValueInserted = 0,
            AveragePrice = 0,
        });
    }

    [Test]
    public void Process_when_buying_and_selling_should_return_reports_for_each_year()
    {
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, quantity: 2) with { Date = new DateTime(2022, 01, 01) },
            Tesla(StockTransactionType.Sell) with { Date = new DateTime(2022, 01, 01) },
            Tesla(StockTransactionType.Sell) with { Date = new DateTime(2023, 01, 01) },
        };

        var (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        reports.Should().HaveCount(2).And.SatisfyRespectively(
            report => report.Year.Should().Be(2022),
            report => report.Year.Should().Be(2023)
        );
    }

    [Test]
    public void TestGainsCalculation()
    {
        var (reports, stocks) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
        });

        reports.First().Gains.Should().Be(0);
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 2,
            ValueInserted = 40,
            AveragePrice = 20,
        });

        (reports, stocks) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
            Tesla(StockTransactionType.Sell, 30),
        });

        reports.First().Gains.Should().Be(10);
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 20,
            AveragePrice = 20,
        });

        (reports, stocks) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
            Tesla(StockTransactionType.Sell, 30),
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Sell, 30),
        });

        reports.First().Gains.Should().Be(25);
        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 15,
            AveragePrice = 15,
        });

        (reports, stocks) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Buy, 30),
            Tesla(StockTransactionType.Sell, 30),
            Tesla(StockTransactionType.Buy, 10),
            Tesla(StockTransactionType.Sell, 30),
            Tesla(StockTransactionType.Sell, 10),
        });

        reports.First().Gains.Should().Be(20);
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
        var stockTransactions = new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10),
        };

        var (_, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        stocks.First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 10,
            AveragePrice = 10,
        });

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

        (_, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

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

        (var reports, stocks) = _stockTransactionService.GetAnnualReports(stockTransactions);

        reports.First().Gains.Should().Be(0);
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
        var (reports, _) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
        });

        reports.First().GainsInEuro.Should().Be(5);

        (reports, _) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
            Tesla(StockTransactionType.Sell, 20, fxRate: 1),
        });

        reports.First().GainsInEuro.Should().Be(15);

        (reports, _) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
            Tesla(StockTransactionType.Sell, 20, fxRate: 1),
            Tesla(StockTransactionType.Sell, 20, fxRate: 0.5m),
        });

        reports.First().GainsInEuro.Should().Be(35);

        (reports, _) = _stockTransactionService.GetAnnualReports(new List<StockTransaction>
        {
            Tesla(StockTransactionType.Buy, 10, quantity: 3),
            Tesla(StockTransactionType.Sell, 20, fxRate: 2),
            Tesla(StockTransactionType.Sell, 20, fxRate: 1),
            Tesla(StockTransactionType.Sell, 20, fxRate: 0.5m),
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
        });

        reports.First().Should().BeEquivalentTo(new StockAnnualReport
        {
            Year = DateTime.Today.Year,
            Gains = 30,
            GainsInEuro = 35,
            CustodyFee = 100,
            CustodyFeeInEuro = 200,
            StockSellOrders = Array.Empty<StockSellOrder>(),
            CashTopUp = 100,
            CashWithdrawal = 0,
            CashTopUpInEuro = 50,
            CashWithdrawalInEuro = 0,
            Dividends = 100,
            DividendsInEuro = 100,
        }, options => options.Excluding(o => o.StockSellOrders));
    }

    [Test]
    public void Test_cash_withdrawal_transaction_behaviour()
    {
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

        var (reports, _) = _stockTransactionService.GetAnnualReports(stockTransactions);

        reports.First().Should().BeEquivalentTo(new StockAnnualReport
        {
            Year = DateTime.Today.Year,
            Gains = 0,
            Dividends = 0,
            CashTopUp = 200,
            CashWithdrawal = 200,
            CustodyFee = 0,
            GainsInEuro = 0,
            DividendsInEuro = 0,
            CashTopUpInEuro = 400,
            CashWithdrawalInEuro = 100,
            CustodyFeeInEuro = 0,
            StockSellOrders = Array.Empty<StockSellOrder>(),
        });
    }
}