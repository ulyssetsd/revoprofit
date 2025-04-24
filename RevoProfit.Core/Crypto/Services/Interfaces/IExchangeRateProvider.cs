namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface IExchangeRateProvider
{
    Task InitializeAsync(bool webAssembly = true);
    decimal GetUsdToEurRate(DateOnly date);
}