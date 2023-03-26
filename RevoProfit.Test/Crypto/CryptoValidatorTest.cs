using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services;

namespace RevoProfit.Test.Crypto;

public class CryptoValidatorTest
{
    private CryptoTransaction _transaction = null!;
    private CryptoTransactionValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CryptoTransactionValidator();
        _transaction = new CryptoTransaction
        {
            Type = CryptoTransactionType.Buy,
            Date = new DateTime(2023, 03, 06, 12, 31, 10),
            BuyAmount = 1,
            BuySymbol = "BTC",
            BuyPrice = 10,
            SellAmount = 1,
            SellSymbol = "ETH",
            SellPrice = 10,
            FeesAmount = 1,
            FeesSymbol = "EUR",
            FeesPrice = 1,
        };
    }

    [Test]
    public void Validate_should_be_valid()
    {
        var result = _validator.IsValid(_transaction);

        result.Should().BeTrue();
    }

    [TestCase(CryptoTransactionType.Buy)]
    [TestCase(CryptoTransactionType.Exchange)]
    public void Validate_when_buy_or_exchange_type_and_buy_amount_zero_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            BuyAmount = 0,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Buy)]
    [TestCase(CryptoTransactionType.Exchange)]
    public void Validate_when_buy_or_exchange_type_and_buy_price_zero_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            BuyPrice = 0,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Buy)]
    [TestCase(CryptoTransactionType.Exchange)]
    public void Validate_when_buy_or_exchange_type_and_buy_symbol_empty_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            BuySymbol = string.Empty,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Sell)]
    [TestCase(CryptoTransactionType.Exchange)]
    public void Validate_when_sell_or_exchange_type_and_sell_amount_zero_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            SellAmount = 0,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Sell)]
    [TestCase(CryptoTransactionType.Exchange)]
    public void Validate_when_sell_or_exchange_type_and_sell_price_zero_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            SellPrice = 0,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Sell)]
    [TestCase(CryptoTransactionType.Exchange)]
    public void Validate_when_sell_or_exchange_type_and_sell_symbol_empty_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            SellSymbol = string.Empty,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Buy)]
    [TestCase(CryptoTransactionType.Sell)]
    [TestCase(CryptoTransactionType.Exchange)]
    [TestCase(CryptoTransactionType.FeesOnly)]
    public void Validate_when_any_type_and_fees_amount_not_zero_and_fees_symbol_empty_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            FeesSymbol = string.Empty,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(CryptoTransactionType.Buy)]
    [TestCase(CryptoTransactionType.Sell)]
    [TestCase(CryptoTransactionType.Exchange)]
    [TestCase(CryptoTransactionType.FeesOnly)]
    public void Validate_when_any_type_and_fees_amount_not_zero_and_fees_price_zero_should_not_be_validated(CryptoTransactionType type)
    {
        // Arrange
        _transaction = _transaction with
        {
            Type = type,
            FeesPrice = 0,
        };

        // Act
        var result = _validator.IsValid(_transaction);

        // Assert
        result.Should().BeFalse();
    }
}