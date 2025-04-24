I have this error when using my input file for the stock [stock input file](../../.csv/trading-account-statement_2020-03-10_2024-12-31_en-us_891b6e.csv): 
```log
RevoProfit.WebAssembly.Components.Stock.StockFileInput[0]
      Quelque chose s'est mal passé lors de la lecture du csv
RevoProfit.Core.Exceptions.ProcessException: fail to map the following line due to a fail to parse StockTransactionType: CUSTODY FEE REVERSAL: StockTransactionCsvLine { Date = 2022-12-14T15:09:22.290878Z, Ticker = , Type = CUSTODY FEE REVERSAL, Quantity = , PricePerShare = , TotalAmount = $1.06, Currency = USD, FxRate = 1.0657 }
   at RevoProfit.Core.Stock.Services.StockTransactionMapper.Map(StockTransactionCsvLine source) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionMapper.cs:line 27
   at System.Linq.Enumerable.SelectListIterator`2[[RevoProfit.Core.Stock.Models.StockTransactionCsvLine, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at System.Collections.Generic.EnumerableHelpers.ToArray[StockTransaction](IEnumerable`1 source, Int32& length)
   at System.Linq.Buffer`1[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]..ctor(IEnumerable`1 source)
   at System.Linq.OrderedEnumerable`1.<GetEnumerator>d__4[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at RevoProfit.Core.Stock.Services.StockTransactionService.GetAnnualReports(IEnumerable`1 stockTransactions) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionService.cs:line 22
   at RevoProfit.WebAssembly.Components.Stock.StockFileInput.SeeResults() in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.WebAssembly\Components\Stock\StockFileInput.razor:line 52
```

can you add a failing test on my test project for the stock and fix the failing error ? thanks

CUSTODY FEE REVERSAL should be removing the amount of fee from the overall fee amount the program calculate, create a new value on the transaction type enum and implement the changes, keep the structure as what done at the moment so you keep it as clean a possible, i did my code to be easily incremental so make sure this works well, if you do change dont forget to add tests for new behaviour