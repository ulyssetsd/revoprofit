using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Extensions;
using RevoProfit.Core.Revolut.Services.Interfaces;
using RevoProfit.Core.Stock.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCoreServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/stock", async (HttpRequest request, IStockCsvService stockCsvService, IStockTransactionService stockTransactionService) =>
{
    var file = (await request.ReadFormAsync()).Files.Single();
    var transactions = await stockCsvService.ReadCsv(file.OpenReadStream());
    var (stockAnnualReports, ownedStocks) = stockTransactionService.GetAnnualReports(transactions);
    return Results.Ok(new { stockAnnualReports, ownedStocks });
}).WithOpenApi();

app.MapPost("/crypto", async (HttpRequest request, IRevolutCsvService cryptoCsvService, IRevolutService revolutService, ICryptoService cryptoService) =>
{
    var file = (await request.ReadFormAsync()).Files.Single();
    var transactions = await cryptoCsvService.ReadCsv(file.OpenReadStream());
    var (cryptoAssets, cryptoSells, cryptoFiatFees) = revolutService.ProcessTransactions(transactions);
    var cryptoReports = cryptoService.MapToReports(cryptoSells, cryptoFiatFees);
    return Results.Ok(new { cryptoAssets, cryptoSells, cryptoReports, cryptoFiatFees });
}).WithOpenApi();

app.Run();
