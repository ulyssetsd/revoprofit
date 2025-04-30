using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Core.Crypto.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private const int DecimalPrecision = 2;

    public CurrencyService(IExchangeRateProvider exchangeRateProvider)
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
        return Math.Round(amount * rate, DecimalPrecision);
    }

    public decimal ConvertFromEur(decimal amount, Currency currency, DateOnly date)
    {
        var rate = _exchangeRateProvider.GetEurRate(date, currency);
        return Math.Round(amount / rate, DecimalPrecision);
    }
}
