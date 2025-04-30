using System.Globalization;
using System.Xml.Linq;
using RevoProfit.Core.CurrencyRate.Models;
using RevoProfit.Core.CurrencyRate.Services.Interfaces;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Core.CurrencyRate.Services;

public class EuropeanCentralBankExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly EuropeanCentralBankUrl _europeanCentralBankUrl;
    private Dictionary<DateOnly, Dictionary<Currency, decimal>>? _historicalRates;

    public EuropeanCentralBankExchangeRateProvider(EuropeanCentralBankUrl europeanCentralBankUrl)
    {
        _httpClient = new HttpClient();
        _europeanCentralBankUrl = europeanCentralBankUrl;
    }

    public async Task InitializeAsync()
    {
        _historicalRates ??= await GetHistoricalRates();
    }

    private async Task<Dictionary<DateOnly, Dictionary<Currency, decimal>>> GetHistoricalRates()
    {
        var historicalRates = new Dictionary<DateOnly, Dictionary<Currency, decimal>>();

        var xmlContent = await _httpClient.GetStringAsync(_europeanCentralBankUrl.Url);
        var xdoc = XDocument.Parse(xmlContent);

        XNamespace ecb = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";
        
        foreach (var dailyRateNode in xdoc.Descendants(ecb + "Cube"))
        {
            var dateValue = dailyRateNode.Attribute("time")?.Value;
            if (!DateOnly.TryParse(dateValue, out DateOnly date)) continue;

            var rates = new Dictionary<Currency, decimal>();
            foreach (var rateElement in dailyRateNode.Elements(ecb + "Cube"))
            {
                var currencyStr = rateElement.Attribute("currency")?.Value;
                var rateValue = rateElement.Attribute("rate")?.Value;
                
                if (Enum.TryParse<Currency>(currencyStr, out var currency) && 
                    decimal.TryParse(rateValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal eurToRate))
                {
                    rates[currency] = eurToRate;
                }
            }

            if (rates.Count != 0)
            {
                historicalRates[date] = rates;
            }
        }
        
        return historicalRates;
    }

    public decimal GetEurRate(DateOnly date, Currency currency)
    {
        var rates = GetRatesForDate(date);
        if (!rates.TryGetValue(currency, out var rate))
        {
            throw new ProcessException($"Exchange rate not found for {currency} to EUR on date {date}");
        }
        
        return rate;
    }

    private Dictionary<Currency, decimal> GetRatesForDate(DateOnly date)
    {
        if (_historicalRates!.TryGetValue(date, out var rates))
        {
            return rates;
        }

        var closestDate = _historicalRates.Keys
            .Where(d => d <= date)
            .OrderByDescending(d => d)
            .FirstOrDefault();

        if (closestDate != default && _historicalRates.TryGetValue(closestDate, out var closestRates))
        {
            return closestRates;
        }

        throw new ProcessException($"No exchange rates found for date {date} or any previous date");
    }
}