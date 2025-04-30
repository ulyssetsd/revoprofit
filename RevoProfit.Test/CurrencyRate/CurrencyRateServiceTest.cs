using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.CurrencyRate.Models;
using RevoProfit.Core.CurrencyRate.Services;

namespace RevoProfit.Test.CurrencyRate;

public class CurrencyRateServiceTest
{
    private CurrencyRateService _target = null!;
    private MockExchangeRateProvider _mockExchangeRateProvider = null!;

    [SetUp]
    public void Setup()
    {
        _mockExchangeRateProvider = new MockExchangeRateProvider(0.91m); // 1 EUR = 0.91 USD
        _target = new CurrencyRateService(_mockExchangeRateProvider);
    }

    [Test]
    public void ConvertToEur_Should_Return_Same_Amount_For_EUR()
    {
        // Arrange
        var amount = 100m; // EUR
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertToEur(amount, Currency.EUR, date);

        // Assert
        result.Should().Be(100m); // 100 EUR = 100 EUR
    }

    [Test]
    public void ConvertToEur_Should_Convert_USD_To_EUR()
    {
        // Arrange
        var amount = 91m; // USD
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertToEur(amount, Currency.USD, date);

        // Assert
        result.Should().BeApproximately(100m, 0.01m); // 91 USD = 100 EUR approximately
    }

    [Test]
    public void ConvertFromEur_Should_Convert_EUR_To_USD()
    {
        // Arrange
        var amount = 100m; // EUR
        var date = new DateOnly(2025, 4, 24);

        // Act
        var result = _target.ConvertFromEur(amount, Currency.USD, date);

        // Assert
        result.Should().BeApproximately(91m, 0.01m); // 100 EUR = 91 USD approximately
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
