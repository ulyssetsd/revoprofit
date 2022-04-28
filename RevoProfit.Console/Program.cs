// See https://aka.ms/new-console-template for more information
using RevoProfit.Core.Crypto;

Console.WriteLine("Hello, World!");

var cryptoService = new CryptoService();
var transactions = await cryptoService.ReadCsv("C:/Users/utass/source/repos/ulyssetsd/revoprofit/crypto_input.csv");
var (cryptos, retraits) = cryptoService.ProcessTransactions(transactions);
foreach (var crypto in cryptos)
{
    Console.WriteLine(crypto);
}

var retraits2021 = retraits.Where(retrait => retrait.Date.Year == 2021);
Console.WriteLine("RETRAITS DE 2021");
foreach (var retrait in retraits2021)
{
    Console.WriteLine(retrait);
}
Console.WriteLine($"Total gains en euros: {retraits2021.Sum(retrait => retrait.GainsEnEuros)}€");