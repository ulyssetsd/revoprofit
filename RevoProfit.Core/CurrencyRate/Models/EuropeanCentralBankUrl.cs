namespace RevoProfit.Core.CurrencyRate.Models;

public record EuropeanCentralBankUrl(string Url)
{
    public static EuropeanCentralBankUrl Default => new("https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml");
    public static EuropeanCentralBankUrl CorsProxy => new("https://corsproxy.io/?url=https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml");
}