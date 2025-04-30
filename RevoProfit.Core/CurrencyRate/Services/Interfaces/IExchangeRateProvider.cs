using RevoProfit.Core.CurrencyRate.Models;

namespace RevoProfit.Core.CurrencyRate.Services.Interfaces;

public interface IExchangeRateProvider
{
    Task InitializeAsync();
    decimal GetEurRate(DateOnly date, Currency currency);
}