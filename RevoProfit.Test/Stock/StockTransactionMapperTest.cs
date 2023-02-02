using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services;

namespace RevoProfit.Test.Stock;

public class StockTransactionMapperTest
{
    private StockTransactionMapper _mapper = null!;

    [SetUp]
    public void Setup()
    {
        _mapper = new StockTransactionMapper();
    }

    [Test]
    [SetCulture("en-GB")]
    public void TestMapping()
    {
        var csvLine = new TransactionCsvLine
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

        var transaction = _mapper.Map(csvLine);

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
        var csvLine = new TransactionCsvLine
        {
            Type = "CASH TOP-UP",
            Quantity = string.Empty,
            PricePerShare = string.Empty,
            TotalAmount = string.Empty,
            Currency = "USD",
            FxRate = string.Empty,
            Date = "10/03/2020 17:48:01",
            Ticker = string.Empty
        };

        var transaction = _mapper.Map(csvLine);

        transaction.Should().BeEquivalentTo(new Transaction
        {
            Type = TransactionType.CashTopUp,
            Quantity = 0,
            PricePerShare = 0,
            TotalAmount = 0,
            FxRate = 0,
            Currency = Currency.Usd,
            Date = new DateTime(2020, 03, 10, 17, 48, 01),
            Ticker = string.Empty,
        });
    }

    [TestCase("BUY", TransactionType.Buy)]
    [TestCase("BUY - MARKET", TransactionType.Buy)]
    [TestCase("BUY - STOP", TransactionType.Buy)]
    [TestCase("CASH TOP-UP", TransactionType.CashTopUp)]
    [TestCase("CUSTODY_FEE" , TransactionType.CustodyFee)]
    [TestCase("CUSTODY FEE", TransactionType.CustodyFee)]
    [TestCase("DIVIDEND", TransactionType.Dividend)]
    [TestCase("SELL", TransactionType.Sell)]
    [TestCase("SELL - MARKET", TransactionType.Sell)]
    [TestCase("SELL - STOP", TransactionType.Sell)]
    [TestCase("STOCK SPLIT", TransactionType.StockSplit)]
    [TestCase("CASH WITHDRAWAL", TransactionType.CashWithdrawal)]
    public void TestEnumMapping(string type, TransactionType expectedType)
    {
        var csvLine = new TransactionCsvLine
        {
            Type = type,
            Quantity = string.Empty,
            PricePerShare = string.Empty,
            TotalAmount = string.Empty,
            Currency = "USD",
            FxRate = string.Empty,
            Date = "10/03/2020 17:48:01",
            Ticker = string.Empty
        };

        var transaction = _mapper.Map(csvLine);

        transaction.Type.Should().Be(expectedType);
    }
}