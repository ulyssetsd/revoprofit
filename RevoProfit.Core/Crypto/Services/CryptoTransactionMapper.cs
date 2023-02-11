using System.Globalization;
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
            MontantRecu = ToDecimal(source.MontantRecu),
            MonnaieOuJetonRecu = source.MonnaieOuJetonRecu,
            MontantEnvoye = ToDecimal(source.MontantEnvoye),
            MonnaieOuJetonEnvoye = source.MonnaieOuJetonEnvoye,
            Frais = ToDecimal(source.Frais),
            MonnaieOuJetonDesFrais = source.MonnaieOuJetonDesFrais,
            PrixDuJetonDuMontantEnvoye = ToDecimal(source.PrixDuJetonDuMontantEnvoye),
            PrixDuJetonDuMontantRecu = ToDecimal(source.PrixDuJetonDuMontantRecu),
            PrixDuJetonDesFrais = ToDecimal(source.PrixDuJetonDesFrais),
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
        "Dépôt" => CryptoTransactionType.Depot,
        "Retrait" => CryptoTransactionType.Retrait,
        "Échange" => CryptoTransactionType.Echange,
        _ => throw new ArgumentOutOfRangeException(),
    };
}