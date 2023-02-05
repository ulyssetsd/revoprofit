﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services;

namespace RevoProfit.Test.Revolut;

internal class RevolutCsvServiceTest
{
    private RevolutCsvService _revolutCsvService = null!;

    [SetUp]
    public void Setup()
    {
        _revolutCsvService = new RevolutCsvService(new RevolutTransactionMapper());
    }

    [Test]
    public async Task ReadCsv_WhenAValidContentIsGiven_ShouldReturnAMappedListOfTheContent()
    {
        // Arrange
        var content = new StringBuilder()
            .AppendLine("Type,Product,Started Date,Completed Date,Description,Amount,Currency,Fiat amount,Fiat amount (inc. fees),Fee,Base currency,State,Balance")
            .AppendLine("CASHBACK,Savings,2022-08-24 3:47:33,2022-08-25 16:28:23,Metal Cashback,0.00000014,BTC,0.01,0.01,0,EUR,COMPLETED,0.00225287")
            .AppendLine("TRANSFER,Current,2021-12-10 8:09:00,2021-12-10 8:09:00,Balance migration to another region or legal entity,1.19357501,ETH,4318.84,4318.84,0,EUR,COMPLETED,")
            .AppendLine("EXCHANGE,Current,2022-05-09 11:24:31,2022-05-09 11:24:31,Exchanged to USD,-3.33720027,WLUNA,-190.44,-187.58,2.85,EUR,COMPLETED,0")
            .AppendLine("CARD_PAYMENT,Current,2018-07-19 15:52:15,2018-07-20 5:28:14,Hotel On Booking.com,-0.00893541,BTC,-56.53,-56.53,0,EUR,COMPLETED,0.00819571")
            .AppendLine("CARD_REFUND,Current,2018-08-21 10:49:04,2018-08-21 19:20:13,Refund from Hotel On Booking.com,0.00893541,BTC,50.01,50.01,0,EUR,COMPLETED,0.00893541")
            .ToString();
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var revolutTransactions = (await _revolutCsvService.ReadCsv(memoryStream)).ToArray();

        // Assert
        revolutTransactions.Should().BeEquivalentTo(new RevolutTransaction[]
        {
            new()
            {
                Type = RevolutTransactionType.CashBack,
                CompletedDate = new DateTime(2022, 08, 25, 16, 28, 23),
                Description = "Metal Cashback",
                Amount = new decimal(0.00000014),
                Currency = "BTC",
                FiatAmount = new decimal(0.01),
                FiatAmountIncludingFees = new decimal(0.01),
                Fee = 0,
                BaseCurrency = "EUR",
                Balance = new decimal(0.00225287)
            },
            new()
            {
                Type = RevolutTransactionType.Transfer,
                CompletedDate = new DateTime(2021, 12, 10, 8, 09, 00),
                Description = "Balance migration to another region or legal entity",
                Amount = new decimal(1.19357501),
                Currency = "ETH",
                FiatAmount = new decimal(4318.84),
                FiatAmountIncludingFees = new decimal(4318.84),
                Fee = 0,
                BaseCurrency = "EUR",
                Balance = null,
            },
            new()
            {
                Type = RevolutTransactionType.Exchange,
                CompletedDate = new DateTime(2022, 05, 09, 11, 24, 31),
                Description = "Exchanged to USD",
                Amount = new decimal(-3.33720027),
                Currency = "WLUNA",
                FiatAmount = new decimal(-190.44),
                FiatAmountIncludingFees = new decimal(-187.58),
                Fee = new decimal(2.85),
                BaseCurrency = "EUR",
                Balance = 0
            },
            new()
            {
                Type = RevolutTransactionType.CardPayment,
                CompletedDate = new DateTime(2018, 07, 20, 05, 28, 14),
                Description = "Hotel On Booking.com",
                Amount = new decimal(-0.00893541),
                Currency = "BTC",
                FiatAmount = new decimal(-56.53),
                FiatAmountIncludingFees = new decimal(-56.53),
                Fee = 0,
                BaseCurrency = "EUR",
                Balance = new decimal(0.00819571)
            },
            new()
            {
                Type = RevolutTransactionType.CardRefund,
                CompletedDate = new DateTime(2018, 08, 21, 19 ,20 ,13),
                Description = "Refund from Hotel On Booking.com",
                Amount = new decimal(0.00893541),
                Currency = "BTC",
                FiatAmount = new decimal(50.01),
                FiatAmountIncludingFees = new decimal(50.01),
                Fee = 0,
                BaseCurrency = "EUR",
                Balance = new decimal(0.00893541)
            },
        });
    }
}