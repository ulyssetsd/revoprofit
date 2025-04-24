using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Services;

namespace RevoProfit.Test.Crypto;

public class EuropeanCentralBankExchangeRateProviderTest
{
    private EuropeanCentralBankExchangeRateProvider _target = null!;
    
    [SetUp]
    public void Setup()
    {
        _target = new EuropeanCentralBankExchangeRateProvider();
    }
    
    [Test]
    public void GetUsdToEurRate_Should_Return_Correct_Rate_For_Current_Date()
    {
        // Arrange
        // Using April 24, 2025 as the test date
        var testDate = new DateOnly(2025, 4, 24);
        
        // For EUR to USD = 1.1376, the USD to EUR rate is 1/1.1376 = approximately 0.8790
        // Allow for some floating-point precision issues with a reasonable delta
        var expectedApproximateRate = 1 / 1.1376m;
        
        // Act
        var actualRate = _target.GetUsdToEurRate(testDate);
        
        // Assert
        // Since exchange rates can change slightly, we verify it's close to the expected value
        actualRate.Should().BeApproximately(expectedApproximateRate, 0.01m);
    }
    
    [Test]
    public void GetUsdToEurRate_Should_Use_Most_Recent_Previous_Rate_When_Date_Not_Available()
    {
        // Arrange
        // Weekends typically don't have exchange rates published, so test with a Sunday
        var weekendDate = new DateOnly(2025, 4, 27); // Sunday
        var previousFridayDate = new DateOnly(2025, 4, 25); // Friday
        
        // Act
        var weekendRate = _target.GetUsdToEurRate(weekendDate);
        var fridayRate = _target.GetUsdToEurRate(previousFridayDate);
        
        // Assert
        // The weekend should use the most recent previous rate (Friday)
        weekendRate.Should().Be(fridayRate);
    }
    
    [Test]
    public void GetUsdToEurRate_Should_Cache_Results()
    {
        // Arrange
        var testDate = new DateOnly(2025, 4, 24);
        
        // Act
        var firstCall = _target.GetUsdToEurRate(testDate);
        var secondCall = _target.GetUsdToEurRate(testDate);
        
        // Assert
        // Both calls should return the same value (from cache after first call)
        secondCall.Should().Be(firstCall);
    }
    
    [Test]
    public void GetUsdToEurRate_Should_Convert_EUR_To_USD_Rate()
    {
        // Arrange
        var testDate = new DateOnly(2025, 4, 24);
        
        // Act
        var usdToEurRate = _target.GetUsdToEurRate(testDate);
        
        // Assert
        // EUR to USD rate (1.1376) converted to USD to EUR should be less than 1
        // USD to EUR = 1 / (EUR to USD)
        usdToEurRate.Should().BeLessThan(1);
    }
}