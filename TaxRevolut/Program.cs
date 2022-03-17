using TaxRevolut.Services;

var crmService = new CsvService();
var transactionService = new TransactionService();
var transactions = crmService.ReadCsv("C:/repo/TaxRevolut/input.csv");
foreach (var transaction in transactions)
{
    transactionService.AddTransaction(transaction);
}

transactionService.GetCurrentStocks().ToList().ForEach(Console.WriteLine);
transactionService.GetSellOrders().ToList().ForEach(Console.WriteLine);
transactionService.GetAnnualGainsReports().ToList().ForEach(Console.WriteLine);