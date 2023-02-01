using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services;

namespace RevoProfit.Test.Stock;

public class CsvServiceTest
{
    private StockCsvService _stockCsvService;
    private const string Headers = "Date,Ticker,Type,Quantity,Price per share,Total Amount,Currency,FX Rate\r\n";

    [SetUp]
    public void Setup()
    {
        _stockCsvService = new StockCsvService();
    }

    [Test]
    [SetCulture("en-US")]
    public async Task test_csv_conversion_is_forcing_culture_toen_gb_even_if_current_culture_is_en_us()
    {
        var content = new StringBuilder()
            .AppendLine(Headers)
            .AppendLine("10/03/2020 17:48:01,,CASH TOP-UP,,,30.00,USD,1.1324686027")
            .ToString();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _stockCsvService.ReadCsv(memoryStream);
        Thread.CurrentThread.CurrentCulture.Should().Be(new CultureInfo("en-US"));
    }

    [Test]
    public async Task test_csv_conversion_is_available_for_en_gb_format_2021()
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
        transactions.ToArray().Should().BeEquivalentTo(new Transaction[]
        {
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 01),
                Ticker = string.Empty,
                Type = TransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 30,
                Currency = Currency.Usd,
                FxRate = 1.1324686027,
            },
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 28),
                Ticker = "BLK",
                Type = TransactionType.Buy,
                Quantity = 0.02245576,
                PricePerShare = 445.32,
                TotalAmount = 10.00,
                Currency = Currency.Usd,
                FxRate = 1.1324878383,
            },
            new()
            {
                Date = new DateTime(2020, 05, 31, 23, 30, 04),
                Ticker = string.Empty,
                Type = TransactionType.CustodyFee,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = -0.01,
                Currency = Currency.Usd,
                FxRate = 1.1118967596,
            },
            new()
            {
                Date = new DateTime(2020, 06, 24, 05, 25, 25),
                Ticker = "BLK",
                Type = TransactionType.Dividend,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 0.63,
                Currency = Currency.Usd,
                FxRate = 1.1317497421,
            },
            new()
            {
                Date = new DateTime(2020, 08, 20, 16, 20, 42),
                Ticker = "TSLA",
                Type = TransactionType.Sell,
                Quantity = 0.0361623,
                PricePerShare = 1991.30,
                TotalAmount = 72.01,
                Currency = Currency.Usd,
                FxRate = 1.1859934110,
            },
            new()
            {
                Date = new DateTime(2021, 07, 20, 10, 35, 47),
                Ticker = "NVDA",
                Type = TransactionType.StockSplit,
                Quantity = 1.5,
                PricePerShare = 0,
                TotalAmount = 0,
                Currency = Currency.Usd,
                FxRate = 1.1795,
            },
        });
    }

    [Test]
    public async Task test_csv_conversion_is_available_for_en_us_format_2022()
    {
        var content = new StringBuilder()
            .AppendLine(Headers)
            .AppendLine("2020-03-10T17:48:01.852420Z,,CASH TOP-UP,,,$30,USD,1.14")
            .AppendLine("2020-03-10T17:48:28.920115Z,BLK,BUY - MARKET,0.02245576,$445.32,$10,USD,1.14")
            .AppendLine("2020-05-31T23:30:04.726579Z,,CUSTODY FEE,,,-$0.01,USD,1.12")
            .AppendLine("2020-06-24T05:28:27.141306Z,BLK,DIVIDEND,,,$0.63,USD,1.14")
            .AppendLine("2020-08-20T16:20:42.271840Z,TSLA,SELL - MARKET,0.0361623,\"$1,991.30\",$72,USD,1.19")
            .AppendLine("2021-07-20T10:35:47.310429Z,NVDA,STOCK SPLIT,1.5,,$0,USD,1.18")
            .ToString();
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var transactions = await _stockCsvService.ReadCsv(memoryStream);
        transactions.ToArray().Should().BeEquivalentTo(new Transaction[]
        {
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 01),
                Ticker = string.Empty,
                Type = TransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 30,
                Currency = Currency.Usd,
                FxRate = 1.1324686027,
            },
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 28),
                Ticker = "BLK",
                Type = TransactionType.Buy,
                Quantity = 0.02245576,
                PricePerShare = 445.32,
                TotalAmount = 10.00,
                Currency = Currency.Usd,
                FxRate = 1.1324878383,
            },
            new()
            {
                Date = new DateTime(2020, 05, 31, 23, 30, 04),
                Ticker = string.Empty,
                Type = TransactionType.CustodyFee,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = -0.01,
                Currency = Currency.Usd,
                FxRate = 1.1118967596,
            },
            new()
            {
                Date = new DateTime(2020, 06, 24, 05, 25, 25),
                Ticker = "BLK",
                Type = TransactionType.Dividend,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 0.63,
                Currency = Currency.Usd,
                FxRate = 1.1317497421,
            },
            new()
            {
                Date = new DateTime(2020, 08, 20, 16, 20, 42),
                Ticker = "TSLA",
                Type = TransactionType.Sell,
                Quantity = 0.0361623,
                PricePerShare = 1991.30,
                TotalAmount = 72.01,
                Currency = Currency.Usd,
                FxRate = 1.1859934110,
            },
            new()
            {
                Date = new DateTime(2021, 07, 20, 10, 35, 47),
                Ticker = "NVDA",
                Type = TransactionType.StockSplit,
                Quantity = 1.5,
                PricePerShare = 0,
                TotalAmount = 0,
                Currency = Currency.Usd,
                FxRate = 1.1795,
            },
        });
    }

    [Test]
    public async Task test_csv_conversion_is_available_for_fr_fr_format_2022()
    {
        var content = new StringBuilder()
            .AppendLine(Headers)
            .AppendLine("2020-03-10T17:48:01.852420Z,,CASH TOP-UP,,,$30,USD,1.14")
            .AppendLine("2020-03-10T17:48:28.920115Z,BLK,BUY - MARKET,0.02245576,$445.32,$10,USD,1.14")
            .AppendLine("2020-05-31T23:30:04.726579Z,,CUSTODY FEE,,,-$0.01,USD,1.12")
            .AppendLine("2020-06-24T05:28:27.141306Z,BLK,DIVIDEND,,,$0.63,USD,1.14")
            .AppendLine("2020-08-20T16:20:42.271840Z,TSLA,SELL - MARKET,0.0361623,\"$1,991.30\",$72,USD,1.19")
            .AppendLine("2021-07-20T10:35:47.310429Z,NVDA,STOCK SPLIT,1.5,,$0,USD,1.18")
            .ToString();
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var transactions = await _stockCsvService.ReadCsv(memoryStream);
        transactions.ToArray().Should().BeEquivalentTo(new Transaction[]
        {
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 01),
                Ticker = string.Empty,
                Type = TransactionType.CashTopUp,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 30,
                Currency = Currency.Usd,
                FxRate = 1.1324686027,
            },
            new()
            {
                Date = new DateTime(2020, 03, 10, 17, 48, 28),
                Ticker = "BLK",
                Type = TransactionType.Buy,
                Quantity = 0.02245576,
                PricePerShare = 445.32,
                TotalAmount = 10.00,
                Currency = Currency.Usd,
                FxRate = 1.1324878383,
            },
            new()
            {
                Date = new DateTime(2020, 05, 31, 23, 30, 04),
                Ticker = string.Empty,
                Type = TransactionType.CustodyFee,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = -0.01,
                Currency = Currency.Usd,
                FxRate = 1.1118967596,
            },
            new()
            {
                Date = new DateTime(2020, 06, 24, 05, 25, 25),
                Ticker = "BLK",
                Type = TransactionType.Dividend,
                Quantity = 0,
                PricePerShare = 0,
                TotalAmount = 0.63,
                Currency = Currency.Usd,
                FxRate = 1.1317497421,
            },
            new()
            {
                Date = new DateTime(2020, 08, 20, 16, 20, 42),
                Ticker = "TSLA",
                Type = TransactionType.Sell,
                Quantity = 0.0361623,
                PricePerShare = 1991.30,
                TotalAmount = 72.01,
                Currency = Currency.Usd,
                FxRate = 1.1859934110,
            },
            new()
            {
                Date = new DateTime(2021, 07, 20, 10, 35, 47),
                Ticker = "NVDA",
                Type = TransactionType.StockSplit,
                Quantity = 1.5,
                PricePerShare = 0,
                TotalAmount = 0,
                Currency = Currency.Usd,
                FxRate = 1.1795,
            },
        });
    }
}