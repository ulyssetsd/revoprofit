using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using TaxRevolut.Core.Models;
using TaxRevolut.Core.Services;

namespace TaxRevolut.Test
{
    public class CsvServiceTest
    {
        private CsvService _csvService;

        [SetUp]
        public void Setup()
        {
            _csvService = new CsvService();
        }

        [Test]
        [SetCulture("en-US")]
        public void TestCsvConversionIsAlwaysMadeInEnGb()
        {
            var stream = GenerateStreamFromString("Date,Ticker,Type,Quantity,Price per share,Total Amount,Currency,FX Rate\r\n10/03/2020 17:48:01,,CASH TOP-UP,,,30.00,USD,1.1324686027");
            var transactions = _csvService.ReadCsv(stream);
            transactions.First().Date.Should().Be(new DateTime(2020, 03, 10, 17, 48, 01));
            Thread.CurrentThread.CurrentCulture.Should().Be(new CultureInfo("en-US"));
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}