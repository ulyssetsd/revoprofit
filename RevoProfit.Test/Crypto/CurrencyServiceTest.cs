using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Test.Crypto;

public class CurrencyServiceTest
{
    private CurrencyService _target = null!;
    private MockExchangeRateProvider _mockExchangeRateProvider = null!;

    [SetUp]
    public void Setup()
    {
        _mockExchangeRateProvider = new MockExchangeRateProvider(0.91m); // 1 USD = 0.91 EUR
        _target = new CurrencyService(_mockExchangeRateProvider);
    }

    [Test]
    public void ConvertToEur_Should_Convert_USD_To_EUR()
    {
        // Arrange
        var amount = 100m; // USD
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertToEur(amount, Currency.USD, date);

        // Assert
        result.Should().Be(91m); // 100 USD = 91 EUR at 0.91 rate
    }

    [Test]
    public void ConvertFromEur_Should_Convert_EUR_To_USD()
    {
        // Arrange
        var amount = 91m; // EUR
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertFromEur(amount, Currency.USD, date);

        // Assert
        result.Should().Be(100m); // 91 EUR = 100 USD at 0.91 rate
    }

    [Test]
    public void ConvertToEur_Should_Round_To_Two_Decimals()
    {
        // Arrange
        var amount = 100.12345m; // USD
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertToEur(amount, Currency.USD, date);

        // Assert
        result.Should().Be(91.11m); // 100.12345 * 0.91 = 91.112340, rounded to 91.11
    }

    [Test] 
    public void ConvertFromEur_Should_Round_To_Two_Decimals()
    {
        // Arrange
        var amount = 91.12345m; // EUR
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertFromEur(amount, Currency.USD, date);

        // Assert
        result.Should().Be(100.14m); // 91.12345 / 0.91 = 100.136758..., rounded to 100.14
    }

    [Test]
    public void ConvertToEur_And_Back_Should_Give_Similar_Results()
    {
        // Arrange
        var originalAmount = 100.00m; // USD
        var date = new DateOnly(2025, 4, 24);

        // Act
        var eurAmount = _target.ConvertToEur(originalAmount, Currency.USD, date);
        var backToUsd = _target.ConvertFromEur(eurAmount, Currency.USD, date);

        // Assert
        backToUsd.Should().BeApproximately(originalAmount, 0.02m);
    }
}
