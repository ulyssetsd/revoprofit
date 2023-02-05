using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Revolut.Models;
using RevoProfit.Core.Revolut.Services;

namespace RevoProfit.Test.Revolut;

public class RevolutMapperTest
{
    private RevolutTransactionMapper _revolutTransactionMapper = null!;

    [SetUp]
    public void Setup()
    {
        _revolutTransactionMapper = new RevolutTransactionMapper();
    }

    private static RevolutTransactionCsvLine GetDefault() => new()
    {
        Type = "CASHBACK",
        CompletedDate = "2021-12-20 12:59:23",
        Description = "description",
        Amount = "0",
        Currency = string.Empty,
        FiatAmount = "0",
        FiatAmountIncludingFees = "0",
        Fee = "0",
        BaseCurrency = string.Empty,
        State = string.Empty,
        Balance = "0",
        Product = string.Empty,
        StartedDate = string.Empty,
    };

    [Test]
    public void Test_revolut_when_values_are_correct_should_map_correctly()
    {
        var csvLine = _revolutTransactionMapper.Map(GetDefault());
        csvLine.Should().BeEquivalentTo(new RevolutTransaction
        {
            Type = RevolutTransactionType.CashBack,
            CompletedDate = new DateTime(2021, 12, 20, 12, 59, 23),
            Description = "description",
            Amount = 0,
            Currency = string.Empty,
            FiatAmount = 0,
            FiatAmountIncludingFees = 0,
            Fee = 0,
            BaseCurrency = string.Empty,
            Balance = 0,
        });
    }

    [TestCase("CASHBACK", RevolutTransactionType.CashBack)]
    [TestCase("EXCHANGE", RevolutTransactionType.Exchange)]
    [TestCase("TRANSFER", RevolutTransactionType.Transfer)]
    [TestCase("CARD_PAYMENT", RevolutTransactionType.CardPayment)]
    [TestCase("CARD_REFUND", RevolutTransactionType.CardRefund)]
    [TestCase("REWARD", RevolutTransactionType.Reward)]
    public void Test_revolut_transaction_type_mapping(string type, RevolutTransactionType expectedType)
    {
        var transaction = _revolutTransactionMapper.Map(GetDefault() with { Type = type });
        transaction.Type.Should().Be(expectedType);
    }

    [Test]
    public void Test_revolut_transaction_type_mapping_should_throw_if_not_matching()
    {
        var act = () => _revolutTransactionMapper.Map(GetDefault() with { Type = "wrongtype" });
        act.Should().Throw<Exception>();
    }

    [Test]
    public void Test_revolut_mapping_when_double_fields_are_empty_should_throw_exception()
    {
        Func<RevolutTransaction> act;
        act = () => _revolutTransactionMapper.Map(GetDefault() with { Amount = string.Empty });
        act.Should().Throw<Exception>();
        act = () => _revolutTransactionMapper.Map(GetDefault() with { FiatAmount = string.Empty });
        act.Should().Throw<Exception>();
        act = () => _revolutTransactionMapper.Map(GetDefault() with { FiatAmountIncludingFees = string.Empty });
        act.Should().Throw<Exception>();
        act = () => _revolutTransactionMapper.Map(GetDefault() with { Fee = string.Empty });
        act.Should().Throw<Exception>();
        act = () => _revolutTransactionMapper.Map(GetDefault() with { Balance = string.Empty });
        act.Should().NotThrow();
    }
}