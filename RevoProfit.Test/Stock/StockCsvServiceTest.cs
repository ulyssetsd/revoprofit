using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services;

namespace RevoProfit.Test.Stock;

public class StockCsvServiceTest
{
    private StockCsvService _stockCsvService = null!;
    private const string Headers = "Date,Ticker,Type,Quantity,Price per share,Total Amount,Currency,FX Rate\r\n";

    [SetUp]
    public void Setup()
    {
        _stockCsvService = new StockCsvService(new StockTransactionMapper());
    }

    [Test]
    public async Task Test_csv_conversion_is_mapping_correctly_for_the_2021_format()
    {
        var content = new StringBuilder()
            .AppendLine(Headers)
            .AppendLine("10/03/2020 17:48:01,,CASH TOP-UP,,,30.00,USD,1.1324686027")
            .AppendLine("10/03/2020 17:48:28,BLK,BUY,0.02245576,445.32,10.00,USD,1.1324878383")
            .AppendLine("31/05/2020 23:30:04,,CUSTODY_FEE,,,-0.01,USD,1.1118967596")
            .AppendLine("24/06/2020 05:25:25,BLK,DIVIDEND,,,0.63,USD,1.1317497421")
            .AppendLine("20/08/2020 16:20:42,TSLA,SELL,0.0361623,1991.30,72.01,USD,1.1859934110")
            .AppendLine("20/07/2021 10:35:47,NVDA,STOCK SPLIT,1.5,,,USD,1.1795")
            .ToString();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var transactions = await _stockCsvService.ReadCsv(memoryStream);
        transactions.ToArray().Should().BeEquivalentTo(new StockTransaction[]
        {
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 01),
                Ticker = string.Empty,
                Type = StockTransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 30,
                FxRate = 1.1324686027m,
            },
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 28),
                Ticker = "BLK",
                Type = StockTransactionType.Buy,
                Quantity = 0.02245576m,
                PricePerShare = 445.32m,
                TotalAmount = 10.00m,
                FxRate = 1.1324878383m,
            },
            new()
            {
                Date = new DateTime(2020, 05, 31, 23, 30, 04),
                Ticker = string.Empty,
                Type = StockTransactionType.CustodyFee,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = -0.01m,
                FxRate = 1.1118967596m,
            },
            new()
            {
                Date = new DateTime(2020, 06, 24, 05, 25, 25),
                Ticker = "BLK",
                Type = StockTransactionType.Dividend,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 0.63m,
                FxRate = 1.1317497421m,
            },
            new()
            {
                Date = new DateTime(2020, 08, 20, 16, 20, 42),
                Ticker = "TSLA",
                Type = StockTransactionType.Sell,
                Quantity = 0.0361623m,
                PricePerShare = 1991.30m,
                TotalAmount = 72.01m,
                FxRate = 1.1859934110m,
            },
            new()
            {
                Date = new DateTime(2021, 07, 20, 10, 35, 47),
                Ticker = "NVDA",
                Type = StockTransactionType.StockSplit,
                Quantity = 1.5m,
                PricePerShare = 0,
                TotalAmount = 0,
                FxRate = 1.1795m,
            },
        });
    }

    [Test]
    public async Task Test_csv_conversion_is_mapping_correctly_for_the_new_2022_format()
    {
        var content = new StringBuilder()
            .AppendLine(Headers)
            .AppendLine("2020-03-10T17:48:01.852420Z,,CASH TOP-UP,,,$30,USD,1.14")
            .AppendLine("2020-03-10T17:48:28.920115Z,BLK,BUY - MARKET,0.02245576,$445.32,$10,USD,1.14")
            .AppendLine("2020-05-31T23:30:04.726579Z,,CUSTODY FEE,,,-$0.01,USD,1.12")
            .AppendLine("2020-06-24T05:28:27.141306Z,BLK,DIVIDEND,,,$0.63,USD,1.14")
            .AppendLine("2020-08-20T16:20:42.271840Z,TSLA,SELL - MARKET,0.0361623,\"$1,991.30\",$72,USD,1.19")
            .AppendLine("2021-01-27T16:14:18.204877Z,GME,SELL - STOP,1,$347.98,$346.76,USD,1.21")
            .AppendLine("2021-07-20T10:35:47.310429Z,NVDA,STOCK SPLIT,1.5,,$0,USD,1.18")
            .AppendLine("2022-11-21T06:46:50.732022Z,,CASH WITHDRAWAL,,,-$800,USD,1.03")
            .ToString();
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var transactions = await _stockCsvService.ReadCsv(memoryStream);
        transactions.ToArray().Should().BeEquivalentTo(new StockTransaction[]
        {
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 01).AddTicks(8524200),
                Ticker = string.Empty,
                Type = StockTransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 30,
                FxRate = 1.14m,
            },
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 28).AddTicks(9201150),
                Ticker = "BLK",
                Type = StockTransactionType.Buy,
                Quantity = 0.02245576m,
                PricePerShare = 445.32m,
                TotalAmount = 10.00m,
                FxRate = 1.14m,
            },
            new()
            {
                Date = new DateTime(2020, 05, 31, 23, 30, 04).AddTicks(7265790),
                Ticker = string.Empty,
                Type = StockTransactionType.CustodyFee,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = -0.01m,
                FxRate = 1.12m,
            },
            new()
            {
                Date = new DateTime(2020, 06, 24, 05, 28, 27).AddTicks(1413060),
                Ticker = "BLK",
                Type = StockTransactionType.Dividend,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 0.63m,
                FxRate = 1.14m,
            },
            new()
            {
                Date = new DateTime(2020, 08, 20, 16, 20, 42).AddTicks(2718400),
                Ticker = "TSLA",
                Type = StockTransactionType.Sell,
                Quantity = 0.0361623m,
                PricePerShare = 1991.30m,
                TotalAmount = 72,
                FxRate = 1.19m,
            },
            new()
            {
                Date = new DateTime(2021, 01, 27, 16, 14, 18).AddTicks(2048770),
                Ticker = "GME",
                Type = StockTransactionType.Sell,
                Quantity = 1,
                PricePerShare = 347.98m,
                TotalAmount = 346.76m,
                FxRate = 1.21m,
            },
            new()
            {
                Date = new DateTime(2021, 07, 20, 10, 35, 47).AddTicks(3104290),
                Ticker = "NVDA",
                Type = StockTransactionType.StockSplit,
                Quantity = 1.5m,
                PricePerShare = 0,
                TotalAmount = 0,
                FxRate = 1.18m,
            },
            new()
            {
                Date = new DateTime(2022, 11, 21, 06, 46, 50).AddTicks(7320220),
                Ticker = string.Empty,
                Type = StockTransactionType.CashWithdrawal,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = -800,
                FxRate = 1.03m,
            },
        });
    }
}