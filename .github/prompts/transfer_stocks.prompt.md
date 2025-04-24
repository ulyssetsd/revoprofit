I have this error when using my input file for the stock [stock input file](../../.csv/trading-account-statement_2020-03-10_2024-12-31_en-us_891b6e.csv): 
```log
fail: RevoProfit.WebAssembly.Components.Stock.StockFileInput[0]
      Something went wrong while reading the csv
RevoProfit.Core.Exceptions.ProcessException: fail to map the following line due to a fail to parse StockTransactionType: TRANSFER FROM REVOLUT TRADING LTD TO REVOLUT SECURITIES EUROPE UAB: StockTransactionCsvLine { Date = 2023-06-25T10:31:23.715038Z, Ticker = DIS, Type = TRANSFER FROM REVOLUT TRADING LTD TO REVOLUT SECURITIES EUROPE UAB, Quantity = 3, PricePerShare = , TotalAmount = $0, Currency = USD, FxRate = 1.0899 }
   at RevoProfit.Core.Stock.Services.StockTransactionMapper.Map(StockTransactionCsvLine source) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionMapper.cs:line 27
   at System.Linq.Enumerable.SelectListIterator`2[[RevoProfit.Core.Stock.Models.StockTransactionCsvLine, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at System.Collections.Generic.EnumerableHelpers.ToArray[StockTransaction](IEnumerable`1 source, Int32& length)
   at System.Linq.Buffer`1[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]..ctor(IEnumerable`1 source)
   at System.Linq.OrderedEnumerable`1.<GetEnumerator>d__4[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at RevoProfit.Core.Stock.Services.StockTransactionService.GetAnnualReports(IEnumerable`1 stockTransactions) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionService.cs:line 22
   at RevoProfit.WebAssembly.Components.Stock.StockFileInput.SeeResults() in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.WebAssembly\Components\Stock\StockFileInput.razor:line 52
```

TRANSFER FROM REVOLUT TRADING LTD TO REVOLUT SECURITIES EUROPE UAB is when revolut did transfer my stocks from the british entity to the eurpoean entity, i'm not sure how to interpret this change, since it was automatic and did not do any selling or buying of stocks, can this be consider as a sell and buy of stock or is it just a transfer and i should continue as nothing happenned on my tax calcul for the french governement ? what do you think ? implement what make the most sens legally

if the transfer stock amount input is not equal to the stock quantity on the program then something is wrong you should log an error

there is also the following error when doing cash transfer

```log
fail: RevoProfit.WebAssembly.Components.Stock.StockFileInput[0]
      fail to map the following line due to a fail to parse StockTransactionType: TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB: StockTransactionCsvLine { Date = 2023-06-25T10:31:23.812473Z, Ticker = , Type = TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB, Quantity = , PricePerShare = , TotalAmount = $571.87, Currency = USD, FxRate = 1.0899 }
RevoProfit.Core.Exceptions.ProcessException: fail to map the following line due to a fail to parse StockTransactionType: TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB: StockTransactionCsvLine { Date = 2023-06-25T10:31:23.812473Z, Ticker = , Type = TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB, Quantity = , PricePerShare = , TotalAmount = $571.87, Currency = USD, FxRate = 1.0899 }
   at RevoProfit.Core.Stock.Services.StockTransactionMapper.Map(StockTransactionCsvLine source) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionMapper.cs:line 27
   at System.Linq.Enumerable.SelectListIterator`2[[RevoProfit.Core.Stock.Models.StockTransactionCsvLine, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at System.Collections.Generic.EnumerableHelpers.ToArray[StockTransaction](IEnumerable`1 source, Int32& length)
   at System.Linq.Buffer`1[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]..ctor(IEnumerable`1 source)
   at System.Linq.OrderedEnumerable`1.<GetEnumerator>d__4[[RevoProfit.Core.Stock.Models.StockTransaction, RevoProfit.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext()
   at RevoProfit.Core.Stock.Services.StockTransactionService.GetAnnualReports(IEnumerable`1 stockTransactions) in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.Core\Stock\Services\StockTransactionService.cs:line 22
   at RevoProfit.WebAssembly.Components.Stock.StockFileInput.SeeResults() in C:\Users\utass\source\repos\ulyssetsd\revoprofit\RevoProfit.WebAssembly\Components\Stock\StockFileInput.razor:line 52
```

TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB with no ticker and no quantity is when there is cash transfer, so if there is cash transfer dont do anything its just an administration tranfser from on revolut account to the other

be careful because having the cash transfer is only when there is not ticket so you cant just gues if its a stock or cash transfer soly on the type of the input it should be interpreted on the service layer

## Implementation Notes

keep the structure as what done at the moment so you keep it as clean a possible, i did my code to be easily incremental so make sure this works well, if you do change dont forget to add tests for new behaviour

# Support for Revolut Stock and Cash Transfers

## Context
When processing Revolut transaction files, two types of transfers need to be handled:

TRANSFER FROM REVOLUT TRADING LTD TO REVOLUT SECURITIES EUROPE UAB
TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB

1. Stock transfers between Revolut entities:
   Example row:
   - Date: 2023-06-25T10:31:23.715038Z
   - Ticker: DIS
   - Type: TRANSFER FROM REVOLUT TRADING LTD TO REVOLUT SECURITIES EUROPE UAB
   - Quantity: 3
   - PricePerShare: empty
   - TotalAmount: $0
   - Currency: USD
   - FxRate: 1.0899

2. Cash transfers between Revolut entities:
   Example row:
   - Date: 2023-06-25T10:31:23.812473Z
   - Ticker: empty
   - Type: TRANSFER FROM REVOLUT BANK UAB TO REVOLUT SECURITIES EUROPE UAB
   - Quantity: empty
   - PricePerShare: empty
   - TotalAmount: $571.87
   - Currency: USD
   - FxRate: 1.0899
