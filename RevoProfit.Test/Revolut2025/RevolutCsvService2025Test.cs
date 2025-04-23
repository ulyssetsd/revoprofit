using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Revolut2025.Services;

namespace RevoProfit.Test.Revolut2025;

internal class RevolutCsvService2025Test
{
    private RevolutCsvService2025 _revolutCsvService2025 = null!;

    [SetUp]
    public void Setup()
    {
        _revolutCsvService2025 = new RevolutCsvService2025(new RevolutTransaction2025Mapper());
    }

    [Test]
    public async Task Map_valid_csv_line_to_revolut_transaction()
    {
        // Arrange
        var content =
        """
        Symbol,Type,Quantity,Price,Value,Fees,Date
        BTC,Buy,0.01713112,"€5,837.33",€100.00,€0.00,"Jun 12, 2018, 4:16:32 PM"
        BTC,Payment,0.00893541,"$7,252.05",$64.80,$0.00,"Jul 20, 2018, 7:28:14 AM"
        BTC,Sell,0.00819571,"€5,504.07",€45.10,€0.00,"Aug 19, 2018, 10:43:55 PM"
        BTC,Other,0.00893541,"$7,252.05",$64.80,$0.00,"Aug 21, 2018, 9:20:13 PM"
        DOT,Send,12.19389828,€23.67,€288.61,€0.00,"Dec 10, 2021, 9:50:53 AM"
        DOT,Receive,12.19389828,€23.67,€288.61,€0.00,"Dec 10, 2021, 9:50:53 AM"
        ETH,Stake,1.26217815,"€1,545.19","€1,950.30",€0.00,"Feb 5, 2023, 1:34:35 PM"
        ETH,Unstake,0.2,"€2,211.73",€442.34,€0.00,"Dec 11, 2023, 12:17:22 AM"
        DOT,Learn reward,0.06188988,,,,"Apr 27, 2022, 11:15:59 AM"
        ETH,Staking reward,0.00002715,,,,"Jul 30, 2023, 1:21:56 AM"
        """;
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var revolutTransactions = (await _revolutCsvService2025.ReadCsv(memoryStream)).ToArray();

        // Assert
        revolutTransactions.Should().BeEquivalentTo([
            new RevolutTransaction2025
            {
                Date = new DateTime(2018, 6, 12, 16, 16, 32),
                Type = RevolutTransactionType.Buy,
                Symbol = "BTC",
                Quantity = 0.01713112m,
                Price = 5837.33m,
                PriceCurrency = "EUR",
                Value = 100.00m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2018, 7, 20, 7, 28, 14),
                Type = RevolutTransactionType.Payment,
                Symbol = "BTC",
                Quantity = 0.00893541m,
                Price = 7252.05m,
                PriceCurrency = "USD",
                Value = 64.80m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2018, 8, 19, 22, 43, 55),
                Type = RevolutTransactionType.Sell,
                Symbol = "BTC",
                Quantity = 0.00819571m,
                Price = 5504.07m,
                PriceCurrency = "EUR",
                Value = 45.10m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2018, 8, 21, 21, 20, 13),
                Type = RevolutTransactionType.Other,
                Symbol = "BTC",
                Quantity = 0.00893541m,
                Price = 7252.05m,
                PriceCurrency = "USD",
                Value = 64.80m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2021, 12, 10, 9, 50, 53),
                Type = RevolutTransactionType.Send,
                Symbol = "DOT",
                Quantity = 12.19389828m,
                Price = 23.67m,
                PriceCurrency = "EUR",
                Value = 288.61m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2021, 12, 10, 9, 50, 53),
                Type = RevolutTransactionType.Receive,
                Symbol = "DOT",
                Quantity = 12.19389828m,
                Price = 23.67m,
                PriceCurrency = "EUR",
                Value = 288.61m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2023, 2, 5, 13, 34, 35),
                Type = RevolutTransactionType.Stake,
                Symbol = "ETH",
                Quantity = 1.26217815m,
                Price = 1545.19m,
                PriceCurrency = "EUR",
                Value = 1950.30m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2023, 12, 11, 0, 17, 22),
                Type = RevolutTransactionType.Unstake,
                Symbol = "ETH",
                Quantity = 0.2m,
                Price = 2211.73m,
                PriceCurrency = "EUR",
                Value = 442.34m,
                Fees = 0.00m,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2022, 4, 27, 11, 15, 59),
                Type = RevolutTransactionType.LearnReward,
                Symbol = "DOT",
                Quantity = 0.06188988m,
                Price = null,
                PriceCurrency = null,
                Value = null,
                Fees = null,
            },
            new RevolutTransaction2025
            {
                Date = new DateTime(2023, 7, 30, 1, 21, 56),
                Type = RevolutTransactionType.StakingReward,
                Symbol = "ETH",
                Quantity = 0.00002715m,
                Price = null,
                PriceCurrency = null,
                Value = null,
                Fees = null,
            },
        ]);
    }

    [Test]
    public async Task Read_csv_with_a_massive_input_2025_format_should_not_throw_any_exception()
    {
        // Arrange & Act
        var act = async () =>
        {
            await using var memoryStream = new FileStream("../../../../.csv/crypto_input_revolut_2025.csv", FileMode.Open);
            return (await _revolutCsvService2025.ReadCsv(memoryStream)).ToArray();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
}