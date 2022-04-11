using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Models;
using RevoProfit.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevoProfit.Test
{
    public class TransactionServiceTest
    {
        private TransactionService _transactionService;
        private int _dateIncrement;

        [SetUp]
        public void Setup()
        {
            _transactionService = new TransactionService();
            _dateIncrement = 0;
        }

        private Transaction Tesla(TransactionType transactionType, double price, double quantity = 1, int yearIncrement = 0, double fxRate = 1)
        {
            return new Transaction
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
            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Buy, 200, yearIncrement: -1),
                Tesla(TransactionType.Sell, 1000, yearIncrement: -1),
            });

            _transactionService.GetAnnualGainsReports().First().Gains.Should().Be(800);
            _transactionService.GetOldStocks().First().Should().BeEquivalentTo(new Stock
            {
                Ticker = "TSLA",
                Quantity = 0,
                ValueInserted = 0,
                AveragePrice = 0,
            });

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Buy, 10),
                Tesla(TransactionType.Buy, 30),
            });

            _transactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().Be(0);
            _transactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new Stock
            {
                Ticker = "TSLA",
                Quantity = 2,
                ValueInserted = 40,
                AveragePrice = 20,
            });

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Sell, 30),
            });

            _transactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().BeApproximately(10, 0.01);
            _transactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new Stock
            {
                Ticker = "TSLA",
                Quantity = 1,
                ValueInserted = 20,
                AveragePrice = 20,
            });

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Buy, 10),
                Tesla(TransactionType.Sell, 30),
            });

            _transactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().BeApproximately(25, 0.01);
            _transactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new Stock
            {
                Ticker = "TSLA",
                Quantity = 1,
                ValueInserted = 15,
                AveragePrice = 15,
            });

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Sell, 10),
            });

            _transactionService.GetAnnualGainsReports().First(report => report.Year == DateTime.Today.Year).Gains.Should().BeApproximately(20, 0.01);
            _transactionService.GetOldStocks().First().Should().BeEquivalentTo(new Stock
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
            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Buy, 10)
            });

            _transactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new Stock
            {
                Ticker = "TSLA",
                Quantity = 1,
                ValueInserted = 10,
                AveragePrice = 10,
            });

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                new()
                {
                    Date = DateTime.Today.AddDays(++_dateIncrement),
                    Ticker = "TSLA",
                    Type = TransactionType.StockSplit,
                    Quantity = 9,
                    Currency = Currency.Usd,
                    FxRate = 1,
                },
            });

            _transactionService.GetCurrentStocks().First().Should().BeEquivalentTo(new Stock
            {
                Ticker = "TSLA",
                Quantity = 10,
                ValueInserted = 10,
                AveragePrice = 1,
            });

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Sell, 1, 10),
            });

            _transactionService.GetAnnualGainsReports().First().Gains.Should().Be(0);
            _transactionService.GetOldStocks().First().Should().BeEquivalentTo(new Stock
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
            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Buy, 10, quantity: 3),
                Tesla(TransactionType.Sell, 20, fxRate: 2),
            });

            _transactionService.GetAnnualGainsReports().First().GainsInEuro.Should().Be(5);

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Sell, 20, fxRate: 1),
            });

            _transactionService.GetAnnualGainsReports().First().GainsInEuro.Should().Be(15);

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                Tesla(TransactionType.Sell, 20, fxRate: 0.5),
            });

            _transactionService.GetAnnualGainsReports().First().GainsInEuro.Should().Be(35);

            _transactionService.ProcessTransactions(new List<Transaction>
            {
                new()
                {
                    Date = DateTime.Today,
                    Type = TransactionType.CustodyFee,
                    TotalAmount = 100,
                    FxRate = 0.5,
                },
                new()
                {
                    Date = DateTime.Today,
                    Type = TransactionType.CashTopUp,
                    TotalAmount = 100,
                    FxRate = 2,
                },
                new()
                {
                    Date = DateTime.Today,
                    Type = TransactionType.Dividend,
                    Ticker = "TSLA",
                    TotalAmount = 100,
                    FxRate = 1,
                },
            });

            _transactionService.GetAnnualGainsReports().First().Should().BeEquivalentTo(new AnnualReport
            {
                Year = DateTime.Today.Year,
                Gains = 30,
                GainsInEuro = 35,
                CustodyFee = 100,
                CustodyFeeInEuro = 200,
                CashTopUp = 100,
                CashTopUpInEuro = 50,
                Dividends = 100,
                DividendsInEuro = 100,
            }, options => options.Excluding(o => o.SellOrders));
        }
    }
}