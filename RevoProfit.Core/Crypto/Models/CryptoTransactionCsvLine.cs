using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Crypto.Models;

public record CryptoTransactionCsvLine
{
    [Name("Type")] public required string Type { get; init; }
    [Name("Date")] public required string Date { get; init; }
    [Name("Fuseau horaire")][Optional] public string? FuseauHoraire { get; init; }
    [Name("Montant reçu")] public required string MontantRecu { get; init; }
    [Name("Monnaie ou jeton reçu")] public required string MonnaieOuJetonRecu { get; init; }
    [Name("Montant envoyé")] public required string MontantEnvoye { get; init; }
    [Name("Monnaie ou jeton envoyé")] public required string MonnaieOuJetonEnvoye { get; init; }
    [Name("Frais")] public required string Frais { get; init; }
    [Name("Monnaie ou jeton des frais")] public required string MonnaieOuJetonDesFrais { get; init; }
    [Name("Exchange / Plateforme")][Optional] public string? ExchangePlateforme { get; init; }
    [Name("Description")][Optional] public string? Description { get; init; }
    [Name("Label")][Optional] public string? Label { get; init; }
    [Name("Prix du jeton du montant envoyé")] public required string PrixDuJetonDuMontantEnvoye { get; init; }
    [Name("Prix du jeton du montant recu")] public required string PrixDuJetonDuMontantRecu { get; init; }
    [Name("Prix du jeton des frais")] public required string PrixDuJetonDesFrais { get; init; }
    [Name("Adresse")][Optional] public string? Adresse { get; init; }
    [Name("Transaction hash")][Optional] public string? TransactionHash { get; init; }
    [Name("ID Externe")][Optional] public string? IdExterne { get; init; }
}