using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto;
using RevoProfit.Core.Mapping;

namespace RevoProfit.Test.CryptoTest;

public class CryptoMapperTest
{
    private Mapper _mapper;

    [SetUp]
    public void Setup()
    {
        _mapper = MapperFactory.GetMapper();
    }

    [TestCase("Dépôt", CryptoTransactionType.Depot)]
    [TestCase("Retrait", CryptoTransactionType.Retrait)]
    [TestCase("Échange", CryptoTransactionType.Echange)]
    public void TestCryptoTransactionTypeEnumMapping(string type, CryptoTransactionType expectedType)
    {
        var transaction = _mapper.Map<CryptoTransaction>(new CryptoTransactionCsvLine { Type = type });
        transaction.Type.Should().Be(expectedType);
    }

    [Test]
    public void TestCryptoTransactionTypeEnumMapping_ShouldThrowIfNotMatchingEnum()
    {
        var act = () => _mapper.Map<CryptoTransaction>(new CryptoTransactionCsvLine { Type = "wrongtype" });
        act.Should().Throw<Exception>();
    }
}