using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using TaxRevolut.Core.Models;
using TaxRevolut.Core.Services;

namespace TaxRevolut.Test
{
    public class MapperTest
    {
        private Mapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = MapperFactory.GetMapper();
        }

        [Test]
        [SetCulture("en-GB")]
        public void TestMapping()
        {
            var csvLine = new CsvLine
            {
                Date = "10/03/2020 17:48:01",
                Ticker = "BLK",
                Type = "CASH TOP-UP",
                Quantity = "0.02245576",
                PricePerShare = "445.32",
                TotalAmount = "10.00",
                Currency = "USD",
                FxRate = "1.1324878383"
            };

            var transaction = _mapper.Map<Transaction>(csvLine);

            transaction.Should().BeEquivalentTo(new Transaction
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 01),
                Ticker = "BLK",
                Type = TransactionType.CashTopUp,
                Quantity = 0.02245576,
                PricePerShare = 445.32,
                TotalAmount = 10.00,
                Currency = Currency.Usd,
                FxRate = 1.1324878383
            });
        }

        [Test]
        public void TestEmptyMapping()
        {
            var csvLine = new CsvLine
            {
                Type = "CASH TOP-UP",
                Quantity = "",
                PricePerShare = "",
                TotalAmount = "",
            };

            var transaction = _mapper.Map<Transaction>(csvLine);

            transaction.Should().BeEquivalentTo(new Transaction
            {
                Type = TransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 0,
            });
        }

        [Test]
        public void TestEnumMapping()
        {
            var csvLines = new List<CsvLine>
            {
                new() { Type = "BUY" },
                new() { Type = "CASH TOP-UP" },
                new() { Type = "CUSTODY_FEE" },
                new() { Type = "DIVIDEND" },
                new() { Type = "SELL" },
                new() { Type = "STOCK SPLIT" },
            };

            var enumsExpectedValues = new List<TransactionType>
            {
                TransactionType.Buy,
                TransactionType.CashTopUp,
                TransactionType.CustodyFee,
                TransactionType.Dividend,
                TransactionType.Sell,
                TransactionType.StockSplit,
            };

            for (var i = 0; i < csvLines.Count; i++)
            {
                var csvLine = csvLines[i];
                var transactionType = enumsExpectedValues[i];

                var transaction = _mapper.Map<Transaction>(csvLine);

                transaction.Type.Should().Be(transactionType);
            }
        }
    }
}