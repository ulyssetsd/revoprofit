using RevoProfit.Core.Globals;
using RevoProfit.Core.Services;
using RevoProfit.Core.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapPost("/uploadcsv", (HttpRequest request, ILogger logger, ICsvService csvService, ITransactionService transactionService) =>
{
    const string fileKey = "formFile";
    if (!request.Form.ContainsKey(fileKey)) return Results.BadRequest();

    try
    {
        var file = request.Form.Files[fileKey];
        var transactions = csvService.ReadCsv(file.OpenReadStream());
        var annualReports = transactionService.GetAnnualReports(transactions);
        logger.LogInformation(LogEvents.GenerateAnnualReports, "Generate annual reports");

        return Results.Ok(annualReports.ToList());
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Issue while generating the annual reports");
        throw;
    }
});

app.Run();