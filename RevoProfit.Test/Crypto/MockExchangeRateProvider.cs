using System;
using System.Threading.Tasks;
using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Test.Crypto;

public class MockExchangeRateProvider : IExchangeRateProvider
{
    private readonly decimal _rate;

    public MockExchangeRateProvider(decimal rate = 0.91m)
    {
        _rate = rate;
    }

    public decimal GetEurRate(DateOnly date, Currency currency)
    {
        return _rate;
    }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }
}