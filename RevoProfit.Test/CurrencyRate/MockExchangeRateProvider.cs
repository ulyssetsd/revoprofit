using System;
using System.Threading.Tasks;
using RevoProfit.Core.CurrencyRate.Models;
using RevoProfit.Core.CurrencyRate.Services.Interfaces;

namespace RevoProfit.Test.CurrencyRate;

public class MockExchangeRateProvider : IExchangeRateProvider
{
    private readonly decimal _rate;

    public MockExchangeRateProvider(decimal rate)
    {
        _rate = rate;
    }

    public decimal GetEurRate(DateOnly date, Currency currency)
    {
        return _rate;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}