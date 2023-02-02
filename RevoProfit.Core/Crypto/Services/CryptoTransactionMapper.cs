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
            MontantRecu = ToDouble(source.MontantRecu),
            MonnaieOuJetonRecu = source.MonnaieOuJetonRecu,
            MontantEnvoye = ToDouble(source.MontantEnvoye),
            MonnaieOuJetonEnvoye = source.MonnaieOuJetonEnvoye,
            Frais = ToDouble(source.Frais),
            MonnaieOuJetonDesFrais = source.MonnaieOuJetonDesFrais,
            ExchangePlateforme = source.ExchangePlateforme,
            Description = source.Description,
            Label = source.Label,
            PrixDuJetonDuMontantEnvoye = ToDouble(source.PrixDuJetonDuMontantEnvoye),
            PrixDuJetonDuMontantRecu = ToDouble(source.PrixDuJetonDuMontantRecu),
            PrixDuJetonDesFrais = ToDouble(source.PrixDuJetonDesFrais)
        };
    }

    private static double ToDouble(string source)
    {
        if (source == string.Empty) return 0;
        return double.Parse(source, CultureInfo.GetCultureInfo("fr-FR"));
    }

    private static CryptoTransactionType ToCryptoTransactionType(string source) => source switch
    {
        "Dépôt" => CryptoTransactionType.Depot,
        "Retrait" => CryptoTransactionType.Retrait,
        "Échange" => CryptoTransactionType.Echange,
        _ => throw new ArgumentOutOfRangeException()
    };
}