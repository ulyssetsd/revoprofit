using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Stock.Services;
using RevoProfit.Core.Stock.Services.Interfaces;
using RevoProfit.WebAssembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<AppState>();
builder.Services.AddScoped<IStockCsvService, StockCsvService>();
builder.Services.AddSingleton<IStockTransactionService, StockStockTransactionService>();
builder.Services.AddScoped<IStockTransactionMapper, StockTransactionMapper>();
builder.Services.AddScoped<ICryptoCsvService, CryptoCsvService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<ICryptoTransactionMapper, CryptoTransactionMapper>();

await builder.Build().RunAsync();
