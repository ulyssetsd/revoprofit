using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using TaxRevolut.Core.Models;
using TaxRevolut.Core.Services;

namespace TaxRevolut.Test
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

        private Transaction Tesla(TransactionType transactionType, double price, double quantity = 1, int yearIncrement = 0)
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
                FxRate = 1,
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
    }
}