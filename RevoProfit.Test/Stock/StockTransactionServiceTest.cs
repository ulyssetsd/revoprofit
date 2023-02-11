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

    private StockTransaction Tesla(TransactionType transactionType, decimal price, decimal quantity = 1, int yearIncrement = 0, decimal fxRate = 1)
    {
        return new StockTransaction
        {
            Date = DateTime.Today.AddYears(yearIncrement).AddDays(++_dateIncrement),
            Ticker = "TSLA",
            Type = transactionType,
            Quantity = quantity,
            PricePerShare = price,
            TotalAmount = price * quantity,
            Currency = Currency.Usd,
            FxRate = fxRate,
        };
    }

    [Test]
    public void TestGainsCalculation()
    {
        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Buy, 200, yearIncrement: -1),
            Tesla(TransactionType.Sell, 1000, yearIncrement: -1),
        });

        _stockTransactionService.GetAnnualGainsReports().First().Gains.Should().Be(800);
        _stockTransactionService.GetOldStocks().First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 0,
            ValueInserted = 0,
            AveragePrice = 0,
        });

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Buy, 10),
            Tesla(TransactionType.Buy, 30),
        });

        _stockTransactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().Be(0);
        _stockTransactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 2,
            ValueInserted = 40,
            AveragePrice = 20,
        });

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Sell, 30),
        });

        _stockTransactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().Be(10);
        _stockTransactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 20,
            AveragePrice = 20,
        });

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Buy, 10),
            Tesla(TransactionType.Sell, 30),
        });

        _stockTransactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().Be(25);
        _stockTransactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 15,
            AveragePrice = 15,
        });

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Sell, 10),
        });

        _stockTransactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().Be(20);
        _stockTransactionService.GetOldStocks().First().Should().BeEquivalentTo(new StockOwned
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
        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Buy, 10),
        });

        _stockTransactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 1,
            ValueInserted = 10,
            AveragePrice = 10,
        });

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            new()
            {
                Date = DateTime.Today.AddDays(++_dateIncrement),
                Ticker = "TSLA",
                Type = TransactionType.StockSplit,
                Quantity = 9,
                Currency = Currency.Usd,
                FxRate = 1,
                PricePerShare = 0,
                TotalAmount = 0,
            },
        });

        _stockTransactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new StockOwned
        {
            Ticker = "TSLA",
            Quantity = 10,
            ValueInserted = 10,
            AveragePrice = 1,
        });

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Sell, 1, 10),
        });

        _stockTransactionService.GetAnnualGainsReports().First().Gains.Should().Be(0);
        _stockTransactionService.GetOldStocks().First().Should().BeEquivalentTo(new StockOwned
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
        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Buy, 10, quantity: 3),
            Tesla(TransactionType.Sell, 20, fxRate: 2),
        });

        _stockTransactionService.GetAnnualGainsReports().First().GainsInEuro.Should().Be(5);

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Sell, 20, fxRate: 1),
        });

        _stockTransactionService.GetAnnualGainsReports().First().GainsInEuro.Should().Be(15);

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            Tesla(TransactionType.Sell, 20, fxRate: 0.5m),
        });

        _stockTransactionService.GetAnnualGainsReports().First().GainsInEuro.Should().Be(35);

        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            new()
            {
                Date = DateTime.Today,
                Type = TransactionType.CustodyFee,
                TotalAmount = 100,
                FxRate = 0.5m,
                Ticker = string.Empty,
                Quantity = 0,
                PricePerShare = 0,
                Currency = Currency.Usd,
            },
            new()
            {
                Date = DateTime.Today,
                Type = TransactionType.CashTopUp,
                TotalAmount = 100,
                FxRate = 2,
                Ticker = string.Empty,
                Quantity = 0,
                PricePerShare = 0,
                Currency = Currency.Usd,
            },
            new()
            {
                Date = DateTime.Today,
                Type = TransactionType.Dividend,
                Ticker = "TSLA",
                TotalAmount = 100,
                FxRate = 1,
                Quantity = 0,
                PricePerShare = 0,
                Currency = Currency.Usd,
            },
        });

        _stockTransactionService.GetAnnualGainsReports().First().Should().BeEquivalentTo(new AnnualReport
        {
            Year = DateTime.Today.Year,
            Gains = 30,
            GainsInEuro = 35,
            CustodyFee = 100,
            CustodyFeeInEuro = 200,
            SellOrders = Array.Empty<SellOrder>(),
            CashTopUp = 100,
            CashWithdrawal = 0,
            CashTopUpInEuro = 50,
            CashWithdrawalInEuro = 0,
            Dividends = 100,
            DividendsInEuro = 100,
        }, options => options.Excluding(o => o.SellOrders));
    }

    [Test]
    public void Test_cash_withdrawal_transaction_behaviour()
    {
        _stockTransactionService.ProcessTransactions(new List<StockTransaction>
        {
            new()
            {
                Date = DateTime.Today.AddDays(-1),
                Ticker = string.Empty,
                Type = TransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 200,
                Currency = Currency.Usd,
                FxRate = 0.5m,
            },
            new()
            {
                Date = DateTime.Today.AddDays(0),
                Ticker = string.Empty,
                Type = TransactionType.CashWithdrawal,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 200,
                Currency = Currency.Usd,
                FxRate = 2,
            },
        });

        _stockTransactionService.GetAnnualGainsReports().First().Should().BeEquivalentTo(new AnnualReport
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
            SellOrders = Array.Empty<SellOrder>(),
        });
    }
}