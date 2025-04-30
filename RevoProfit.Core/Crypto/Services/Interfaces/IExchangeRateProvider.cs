namespace RevoProfit.Core.Crypto.Services.Interfaces;

public interface IExchangeRateProvider
{
    Task InitializeAsync();
    decimal GetEurRate(DateOnly date, Currency currency);
}

public enum Currency { EUR, USD, JPY, BGN, CZK, DKK, GBP, HUF, PLN, RON, SEK, CHF, ISK, NOK, TRY, AUD, BRL, CAD, CNY, HKD, IDR, ILS, INR, KRW, MXN, MYR, NZD, PHP, SGD, THB, ZAR }

public static class CurrencyExtensions
{
    public static Currency ToCurrency(this string currencyStr)
    {
        return Enum.TryParse<Currency>(currencyStr, out var currency) ? currency : throw new ArgumentException($"Invalid currency: {currencyStr}");
    }
}

public interface ICurrencyService
{
    decimal ConvertToEur(decimal amount, Currency currency, DateOnly date);
    decimal ConvertFromEur(decimal amount, Currency currency, DateOnly date);
}