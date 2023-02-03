using System;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services;

namespace RevoProfit.Test.Crypto;

public class CryptoMapperTest
{
    private CryptoTransactionMapper _cryptoTransactionMapper = null!;

    [SetUp]
    public void Setup()
    {
        _cryptoTransactionMapper = new CryptoTransactionMapper();
    }

    private static CryptoTransactionCsvLine GetDummyCryptoTransactionCsvLine(string type) => new()
    {
        Date = "12/06/2018 12:16:32",
        MontantRecu = string.Empty,
        MonnaieOuJetonRecu = string.Empty,
        MontantEnvoye = string.Empty,
        MonnaieOuJetonEnvoye = string.Empty,
        Frais = string.Empty,
        MonnaieOuJetonDesFrais = string.Empty,
        PrixDuJetonDuMontantEnvoye = string.Empty,
        PrixDuJetonDuMontantRecu = string.Empty,
        PrixDuJetonDesFrais = string.Empty,
        Type = type,
    };

    [TestCase("Dépôt", CryptoTransactionType.Depot)]
    [TestCase("Retrait", CryptoTransactionType.Retrait)]
    [TestCase("Échange", CryptoTransactionType.Echange)]
    public void TestCryptoTransactionTypeEnumMapping(string type, CryptoTransactionType expectedType)
    {
        var transaction = _cryptoTransactionMapper.Map(GetDummyCryptoTransactionCsvLine(type));
        transaction.Type.Should().Be(expectedType);
    }

    [Test]
    public void TestCryptoTransactionTypeEnumMapping_ShouldThrowIfNotMatchingEnum()
    {
        var act = () => _cryptoTransactionMapper.Map(GetDummyCryptoTransactionCsvLine("wrongtype"));
        act.Should().Throw<Exception>();
    }
}