namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface IExchangeRateProvider
{
    decimal GetUsdToEurRate(DateOnly date);
}