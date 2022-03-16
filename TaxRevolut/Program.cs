using TaxRevolut.Services;

var crmService = new CsvService();
var transactions = crmService.ReadCsv("./input.csv");
foreach (var transaction in transactions)
{
    Console.WriteLine(transaction);
}