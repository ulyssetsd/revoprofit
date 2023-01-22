using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Stock.Services;

namespace RevoProfit.Test.Stock;

public class CsvServiceTest
{
    private StockCsvService _stockCsvService;

    [SetUp]
    public void Setup()
    {
        _stockCsvService = new StockCsvService();
    }

    [Test]
    [SetCulture("en-US")]
    public async Task TestCsvConversionIsAlwaysMadeInEnGb()
    {
        var content = "Date,Ticker,Type,Quantity,Price per share,Total Amount,Currency,FX Rate\r\n10/03/2020 17:48:01,,CASH TOP-UP,,,30.00,USD,1.1324686027";
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var transactions = await _stockCsvService.ReadCsv(memoryStream);
        transactions.First().Date.Should().Be(new DateTime(2020, 03, 10, 17, 48, 01));
        Thread.CurrentThread.CurrentCulture.Should().Be(new CultureInfo("en-US"));
    }
}