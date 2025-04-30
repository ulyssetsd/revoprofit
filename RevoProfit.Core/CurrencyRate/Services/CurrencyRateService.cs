using RevoProfit.Core.CurrencyRate.Models;
using RevoProfit.Core.CurrencyRate.Services.Interfaces;

namespace RevoProfit.Core.CurrencyRate.Services;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public CurrencyRateService(IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRateProvider = exchangeRateProvider;
    }

    public decimal ConvertToEur(decimal amount, Currency currency, DateOnly date)
    {
        if (currency == Currency.EUR)
        {
            return amount;
        }
        var rate = _exchangeRateProvider.GetEurRate(date, currency);
        return amount / rate;
    }

    public decimal ConvertFromEur(decimal amount, Currency currency, DateOnly date)
    {
        var rate = _exchangeRateProvider.GetEurRate(date, currency);
        return amount * rate;
    }
}
