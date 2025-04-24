I have this error when using my input file for the stock [stock input file](../../.csv/trading-account-statement_2020-03-10_2024-12-31_en-us_891b6e.csv): 
```log
fail: RevoProfit.WebAssembly.Components.Stock.StockFileInput[0]
      Something went wrong while reading the csv
RevoProfit.Core.Exceptions.ProcessException: fail to map the following line due to a fail to parse date 2023-02-02T18:32:06.250Z: StockTransactionCsvLine { Date = 2023-02-02T18:32:06.250Z, Ticker = AMD, Type = SELL - MARKET, Quantity = 1, PricePerShare = $88.12, TotalAmount = $88.10, Currency = USD, FxRate = 1.0927 }
   at RevoProfit.Core.Stock.Services.StockTransactionMapper.Map(StockTransactionCsvLine source) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionMapper.cs:line 27
   at System.Linq.Enumerable.SelectListIterator`2[[RevoProfit.Core.Stock.Models.StockTransactionCsvLine, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at System.Collections.Generic.EnumerableHelpers.ToArray[StockTransaction](IEnumerable`1 source, Int32& length)
   at System.Linq.Buffer`1[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]..ctor(IEnumerable`1 source)
   at System.Linq.OrderedEnumerable`1.<GetEnumerator>d__4[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at RevoProfit.Core.Stock.Services.StockTransactionService.GetAnnualReports(IEnumerable`1 stockTransactions) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionService.cs:line 22
   at RevoProfit.WebAssembly.Components.Stock.StockFileInput.SeeResults() in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.WebAssembly\Components\Stock\StockFileInput.razor:line 52
```

can you add a failing test on my test project for the stock and fix the failing error ?
keep the structure as what done at the moment so you keep it as clean a possible, i did my code to be easily incremental so make sure this works well, if you do change dont forget to add tests for new behaviour