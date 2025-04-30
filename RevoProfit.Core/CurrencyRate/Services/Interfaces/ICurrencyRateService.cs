using RevoProfit.Core.CurrencyRate.Models;

namespace RevoProfit.Core.CurrencyRate.Services.Interfaces;

public interface ICurrencyRateService
{
    decimal ConvertToEur(decimal amount, Currency currency, DateOnly date);
    decimal ConvertFromEur(decimal amount, Currency currency, DateOnly date);
}