﻿using System.Globalization;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoTransactionMapper : ICryptoTransactionMapper
{
    public CryptoTransaction Map(CryptoTransactionCsvLine source)
    {
        return new CryptoTransaction
        {
            Type = ToCryptoTransactionType(source.Type),
            Date = DateTime.ParseExact(source.Date, "G", CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None),
            BuyAmount = ToDecimal(source.MontantRecu),
            BuySymbol = source.MonnaieOuJetonRecu,
            BuyPrice = ToDecimal(source.PrixDuJetonDuMontantRecu),
            SellAmount = ToDecimal(source.MontantEnvoye),
            SellSymbol = source.MonnaieOuJetonEnvoye,
            SellPrice = ToDecimal(source.PrixDuJetonDuMontantEnvoye),
            FeesAmount = ToDecimal(source.Frais),
            FeesSymbol = source.MonnaieOuJetonDesFrais,
            FeesPrice = ToDecimal(source.PrixDuJetonDesFrais),
        };
    }

    private static decimal ToDecimal(string source)
    {
        if (source == string.Empty) return 0;
        if (decimal.TryParse(source, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var frResult)) return frResult;
        if (decimal.TryParse(source, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out var enResult)) return enResult;
        throw new InvalidOperationException();
    }

    private static CryptoTransactionType ToCryptoTransactionType(string source) => source switch
    {
        "Dépôt" => CryptoTransactionType.Buy,
        "Retrait" => CryptoTransactionType.Sell,
        "Échange" => CryptoTransactionType.Exchange,
        _ => throw new ArgumentOutOfRangeException(),
    };
}