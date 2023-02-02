using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Crypto.Models;

public class CryptoTransactionCsvLine
{
    [Name("Type")] public string Type { get; init; }
    [Name("Date")] public string Date { get; init; }
    [Name("Fuseau horaire")][Optional] public string? FuseauHoraire { get; init; }
    [Name("Montant reçu")] public string MontantRecu { get; init; }
    [Name("Monnaie ou jeton reçu")] public string MonnaieOuJetonRecu { get; init; }
    [Name("Montant envoyé")] public string MontantEnvoye { get; init; }
    [Name("Monnaie ou jeton envoyé")] public string MonnaieOuJetonEnvoye { get; init; }
    [Name("Frais")] public string Frais { get; init; }
    [Name("Monnaie ou jeton des frais")] public string MonnaieOuJetonDesFrais { get; init; }
    [Name("Exchange / Plateforme")][Optional] public string? ExchangePlateforme { get; init; }
    [Name("Description")][Optional] public string? Description { get; init; }
    [Name("Label")][Optional] public string? Label { get; init; }
    [Name("Prix du jeton du montant envoyé")] public string PrixDuJetonDuMontantEnvoye { get; init; }
    [Name("Prix du jeton du montant recu")] public string PrixDuJetonDuMontantRecu { get; init; }
    [Name("Prix du jeton des frais")] public string PrixDuJetonDesFrais { get; init; }
    [Name("Adresse")][Optional] public string? Adresse { get; init; }
    [Name("Transaction hash")][Optional] public string? TransactionHash { get; init; }
    [Name("ID Externe")][Optional] public string? IdExterne { get; init; }
}