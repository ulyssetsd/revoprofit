using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RevoProfit.Core.Services;
using RevoProfit.Core.Services.Interfaces;
using RevoProfit.WebAssembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<ICsvService, CsvService>();
builder.Services.AddSingleton<ITransactionService, TransactionService>();

await builder.Build().RunAsync();
