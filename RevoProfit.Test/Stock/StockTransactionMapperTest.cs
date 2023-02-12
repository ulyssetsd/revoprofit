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
        var csvLine = new StockTransactionCsvLine
        {
            Date = "10/03/2020 17:48:01",
            Ticker = "BLK",
            Type = "CASH TOP-UP",
            Quantity = "0.02245576",
            PricePerShare = "445.32",
            TotalAmount = "10.00",
            Currency = "USD",
            FxRate = "1.1324878383",
        };

        var transaction = _mapper.Map(csvLine);

        transaction.Should().BeEquivalentTo(new StockTransaction
        {
            Date = new DateTime(2020, 03, 10, 17, 48, 01),
            Ticker = "BLK",
            Type = StockTransactionType.CashTopUp,
            Quantity = 0.02245576m,
            PricePerShare = 445.32m,
            TotalAmount = 10.00m,
            FxRate = 1.1324878383m,
        });
    }

    [Test]
    public void TestEmptyMapping()
    {
        var csvLine = new StockTransactionCsvLine
        {
            Type = "CASH TOP-UP",
            Quantity = string.Empty,
            PricePerShare = string.Empty,
            TotalAmount = string.Empty,
            Currency = "USD",
            FxRate = string.Empty,
            Date = "10/03/2020 17:48:01",
            Ticker = string.Empty,
        };

        var transaction = _mapper.Map(csvLine);

        transaction.Should().BeEquivalentTo(new StockTransaction
        {
            Type = StockTransactionType.CashTopUp,
            Quantity = 0,
            PricePerShare = 0,
            TotalAmount = 0,
            FxRate = 0,
            Date = new DateTime(2020, 03, 10, 17, 48, 01),
            Ticker = string.Empty,
        });
    }

    [TestCase("BUY", StockTransactionType.Buy)]
    [TestCase("BUY - MARKET", StockTransactionType.Buy)]
    [TestCase("BUY - STOP", StockTransactionType.Buy)]
    [TestCase("CASH TOP-UP", StockTransactionType.CashTopUp)]
    [TestCase("CUSTODY_FEE" , StockTransactionType.CustodyFee)]
    [TestCase("CUSTODY FEE", StockTransactionType.CustodyFee)]
    [TestCase("DIVIDEND", StockTransactionType.Dividend)]
    [TestCase("SELL", StockTransactionType.Sell)]
    [TestCase("SELL - MARKET", StockTransactionType.Sell)]
    [TestCase("SELL - STOP", StockTransactionType.Sell)]
    [TestCase("STOCK SPLIT", StockTransactionType.StockSplit)]
    [TestCase("CASH WITHDRAWAL", StockTransactionType.CashWithdrawal)]
    public void TestEnumMapping(string type, StockTransactionType expectedType)
    {
        var csvLine = new StockTransactionCsvLine
        {
            Type = type,
            Quantity = string.Empty,
            PricePerShare = string.Empty,
            TotalAmount = string.Empty,
            Currency = "USD",
            FxRate = string.Empty,
            Date = "10/03/2020 17:48:01",
            Ticker = string.Empty,
        };

        var transaction = _mapper.Map(csvLine);

        transaction.Type.Should().Be(expectedType);
    }
}