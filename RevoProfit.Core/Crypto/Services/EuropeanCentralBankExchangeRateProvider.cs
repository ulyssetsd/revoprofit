using System.Globalization;
using System.Xml.Linq;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Core.Crypto.Services;

public class EuropeanCentralBankExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly EuropeanCentralBankUrl _europeanCentralBankUrl;
    private Dictionary<DateOnly, decimal>? _historicalRates;

    public EuropeanCentralBankExchangeRateProvider(EuropeanCentralBankUrl europeanCentralBankUrl)
    {
        _httpClient = new HttpClient();
        _europeanCentralBankUrl = europeanCentralBankUrl;
    }

    public async Task InitializeAsync()
    {
        _historicalRates ??= await GetHistoricalRates();
    }

    private async Task<Dictionary<DateOnly, decimal>> GetHistoricalRates()
    {
        Dictionary<DateOnly, decimal> historicalRates = [];

        var xmlContent = await _httpClient.GetStringAsync(_europeanCentralBankUrl.Url);
        var xdoc = XDocument.Parse(xmlContent);

        XNamespace ecb = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";
        
        foreach (var dailyRateNode in xdoc.Descendants(ecb + "Cube"))
        {
            var dateValue = dailyRateNode.Attribute("time")?.Value;
            if (!DateOnly.TryParse(dateValue, out DateOnly date)) continue;
            var usdRateElement = dailyRateNode.Elements(ecb + "Cube").FirstOrDefault(r => r.Attribute("currency")?.Value == "USD");
            var usdRateValue = usdRateElement?.Attribute("rate")?.Value;
            if (!decimal.TryParse(usdRateValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal eurToUsd)) continue;
            historicalRates[date] = 1 / eurToUsd;
        }
        
        return historicalRates;
    }

    public decimal GetUsdToEurRate(DateOnly date)
    {
        if (_historicalRates!.TryGetValue(date, out var rate))
        {
            return rate;
        }

        // Try to find the closest previous date if the exact date is not available
        var closestDate = _historicalRates.Keys
            .Where(d => d <= date)
            .OrderByDescending(d => d)
            .FirstOrDefault();

        if (closestDate != default && _historicalRates.TryGetValue(closestDate, out var closestRate))
        {
            return closestRate;
        }

        throw new ProcessException($"Exchange rate not found for USD to EUR on date {date} or any previous date");
    }
}