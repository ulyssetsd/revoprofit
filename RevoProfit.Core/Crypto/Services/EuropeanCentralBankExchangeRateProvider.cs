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
        Dictionary<DateOnly, decimal> historicalRates = new();

        var xmlContent = await _httpClient.GetStringAsync(_europeanCentralBankUrl.Url);
        var xdoc = XDocument.Parse(xmlContent);

        // Define the XML namespaces used in the document
        XNamespace gesmes = "http://www.gesmes.org/xml/2002-08-01";
        XNamespace ecb = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

        // Find the parent Cube element
        var parentCube = xdoc.Descendants(ecb + "Cube").FirstOrDefault(e => e.Elements(ecb + "Cube").Any(x => x.Attribute("time") != null));
        if (parentCube == null)
            return historicalRates;

        // Find all the daily Cube elements that contain rate data
        var dailyCubes = parentCube.Elements(ecb + "Cube").Where(x => x.Attribute("time") != null);

        foreach (var dailyCube in dailyCubes)
        {
            var timeAttr = dailyCube.Attribute("time")?.Value;
            if (DateOnly.TryParse(timeAttr, out DateOnly cubeDate))
            {
                var usdElement = dailyCube.Elements(ecb + "Cube").FirstOrDefault(x => x.Attribute("currency")?.Value == "USD");
                if (usdElement != null && decimal.TryParse(usdElement.Attribute("rate")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal eurToUsd))
                {
                    historicalRates[cubeDate] = 1 / eurToUsd;
                }
            }
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