using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RevoProfit.Core.Extensions;
using RevoProfit.Logging;
using RevoProfit.WebAssembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<AppState>();
builder.Services.AddCoreServices();
builder.Logging.AddProvider(new HeightBaseLoggerProvider());

await builder.Build().RunAsync();