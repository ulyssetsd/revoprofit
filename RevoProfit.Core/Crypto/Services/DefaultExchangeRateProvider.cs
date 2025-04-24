using System.Net.Http.Json;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Core.Crypto.Services;

public class DefaultExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<DateOnly, decimal> _rateCache = [];

    public DefaultExchangeRateProvider()
    {
        _httpClient = new HttpClient();
    }

    public decimal GetUsdToEurRate(DateOnly date)
    {
        if (_rateCache.TryGetValue(date, out var rate))
        {
            return rate;
        }

        var response = _httpClient.GetFromJsonAsync<ExchangeRateResponse>($"https://api.exchangerate-api.com/v4/{date:yyyy-MM-dd}/USD").Result;
        rate = response?.Rates.GetValueOrDefault("EUR") ?? throw new ProcessException("Failed to get exchange rate from API.");

        _rateCache[date] = rate;
        return rate;
    }

    private record ExchangeRateResponse
    {
        public Dictionary<string, decimal> Rates { get; init; } = [];
    }
}