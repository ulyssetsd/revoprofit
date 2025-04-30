using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Revolut.Services.Interfaces;
using RevoProfit.Core.Revolut.Services;
using RevoProfit.Core.Revolut2025.Services;
using RevoProfit.Core.Stock.Services.Interfaces;
using RevoProfit.Core.Stock.Services;
using Microsoft.Extensions.DependencyInjection;
using RevoProfit.Core.Revolut2025.Services.Interfaces;

namespace RevoProfit.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all <see cref="Core"/> services to the service collection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IStockCsvService, StockCsvService>();
        services.AddScoped<IStockTransactionService, StockTransactionService>();
        services.AddScoped<IStockTransactionMapper, StockTransactionMapper>();
        services.AddScoped<ICryptoCsvService, CryptoCsvService>();
        services.AddScoped<ICryptoService, CryptoService>();
        services.AddScoped<ICryptoTransactionMapper, CryptoTransactionMapper>();
        services.AddScoped<ICryptoTransactionValidator, CryptoTransactionFluentValidator>();
        services.AddScoped<IRevolutCsvService, RevolutCsvService>();
        services.AddScoped<IRevolutService, RevolutService>();
        services.AddScoped<IRevolutTransactionMapper, RevolutTransactionMapper>();
        services.AddScoped<IRevolut2025CsvService, Revolut2025CsvService>();
        services.AddScoped<IRevolut2025TransactionMapper, Revolut2025TransactionMapper>();
        services.AddScoped<IRevolut2025Service, Revolut2025Service>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddSingleton<IExchangeRateProvider, EuropeanCentralBankExchangeRateProvider>();
        services.AddSingleton(EuropeanCentralBankUrl.CorsProxy);
    }
}