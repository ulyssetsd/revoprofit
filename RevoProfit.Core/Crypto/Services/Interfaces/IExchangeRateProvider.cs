namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface IExchangeRateProvider
{
    Task InitializeAsync();
    decimal GetUsdToEurRate(DateOnly date);
}