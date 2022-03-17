using TaxRevolut.Services;

var crmService = new CsvService();
var transactionService = new TransactionService();
var transactions = crmService.ReadCsv("C:/repo/TaxRevolut/input.csv");
transactionService.ProcessTransactions(transactions);
transactionService.GetCurrentStocks().ToList().ForEach(Console.WriteLine);
transactionService.GetOldStocks().ToList().ForEach(Console.WriteLine);
transactionService.GetSellOrders().ToList().ForEach(Console.WriteLine);
transactionService.GetAnnualGainsReports().ToList().ForEach(Console.WriteLine);