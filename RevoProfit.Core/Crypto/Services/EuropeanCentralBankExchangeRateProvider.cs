using System.Xml.Linq;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Exceptions;

namespace RevoProfit.Core.Crypto.Services;

public class EuropeanCentralBankExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<DateOnly, decimal> _historicalRates = [];
    private const string EcbHistoricalUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";
    private const string FallbackCorsProxyUrl = $"https://corsproxy.io/?url=https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

    public EuropeanCentralBankExchangeRateProvider()
    {
        _httpClient = new HttpClient();
    }

    public async Task InitializeAsync(bool webAssembly = true)
    {
        string xmlContent = await _httpClient.GetStringAsync(webAssembly ? FallbackCorsProxyUrl : EcbHistoricalUrl);
        var xdoc = XDocument.Parse(xmlContent);

        // Define the XML namespaces used in the document
        XNamespace gesmes = "http://www.gesmes.org/xml/2002-08-01";
        XNamespace ecb = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

        // Find all the daily Cube elements that contain rate data
        var dailyCubes = xdoc.Descendants(ecb + "Cube")
            .Where(x => x.Attribute("time") != null);

        foreach (var dailyCube in dailyCubes)
        {
            if (DateOnly.TryParse(dailyCube.Attribute("time")?.Value, out DateOnly cubeDate))
            {
                // Find the USD rate in this day's data
                var usdElement = dailyCube.Elements(ecb + "Cube")
                    .FirstOrDefault(x => x.Attribute("currency")?.Value == "USD");

                if (usdElement != null && decimal.TryParse(usdElement.Attribute("rate")?.Value, out decimal eurToUsd))
                {
                    // Convert EUR to USD rate to USD to EUR rate (by taking the inverse)
                    _historicalRates[cubeDate] = 1 / eurToUsd;
                }
            }
        }
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