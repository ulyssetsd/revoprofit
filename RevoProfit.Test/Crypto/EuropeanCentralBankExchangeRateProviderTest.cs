using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Test.Crypto;

public class EuropeanCentralBankExchangeRateProviderTest
{
    private EuropeanCentralBankExchangeRateProvider _target = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _target = new EuropeanCentralBankExchangeRateProvider(EuropeanCentralBankUrl.Default);
        await _target.InitializeAsync();
    }

    [Test]
    public void ConvertToEur_Should_Return_Correct_Rate_For_USD()
    {
        // Arrange
        var testDate = new DateOnly(2025, 4, 24);
        
        // Act
        var eurAmount = _target.GetEurRate(testDate, Currency.USD);
        
        // Assert
        // For EUR to USD = 1.1376, 100 EUR = 113.76 USD approximately
        eurAmount.Should().BeApproximately(1.1376m, 0.0001m);
    }

    [Test]
    public void ConvertToEur_Should_Use_Most_Recent_Previous_Rate()
    {
        // Arrange
        var weekendDate = new DateOnly(2025, 4, 27); // Sunday
        var previousFridayDate = new DateOnly(2025, 4, 25); // Friday

        // Act
        var weekendRate = _target.GetEurRate(weekendDate, Currency.USD);
        var fridayRate = _target.GetEurRate(previousFridayDate, Currency.USD);
        
        // Assert
        weekendRate.Should().Be(fridayRate);
    }

    [Test]
    public void GetEurRate_Should_Return_Correct_Rate_For_GBP()
    {
        // Arrange
        var testDate = new DateOnly(2025, 4, 24);
        
        // Act
        var eurAmount = _target.GetEurRate(testDate, Currency.GBP);
        
        // Assert
        // Should return a reasonable value for GBP conversion
        eurAmount.Should().BeGreaterThan(0);
        eurAmount.Should().BeLessThan(1); // GBP is typically stronger than EUR
    }

    [Test]
    public void GetEurRate_Should_Throw_For_Unknown_Currency()
    {
        // Arrange
        var testDate = new DateOnly(2025, 4, 24);
        
        // Act
        var act = () => _target.GetEurRate(testDate, (Currency)999);
        
        // Assert
        act.Should().Throw<ProcessException>()
           .WithMessage("*Exchange rate not found*");
    }
}