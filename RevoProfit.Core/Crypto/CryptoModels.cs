using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Crypto;

public class CryptoTransactionCsvLine
{
    [Name("Type")] public string? Type { get; init; }
    [Name("Date")] public string? Date { get; init; }
    [Name("Montant reçu")] public string? MontantRecu { get; init; }
    [Name("Monnaie ou jeton reçu")] public string? MonnaieOuJetonRecu { get; init; }
    [Name("Montant envoyé")] public string? MontantEnvoye { get; init; }
    [Name("Monnaie ou jeton envoyé")] public string? MonnaieOuJetonEnvoye { get; init; }
    [Name("Frais")] public string? Frais { get; init; }
    [Name("Monnaie ou jeton des frais")] public string? MonnaieOuJetonDesFrais { get; init; }
    [Name("Exchange / Plateforme")] public string? ExchangePlateforme { get; init; }
    [Name("Description")] public string? Description { get; init; }
    [Name("Label")] public string? Label { get; init; }
    [Name("Prix du jeton du montant envoyé")] public string? PrixDuJetonDuMontantEnvoye { get; init; }
    [Name("Prix du jeton du montant recu")] public string? PrixDuJetonDuMontantRecu { get; init; }
    [Name("Prix du jeton des frais")] public string? PrixDuJetonDesFrais { get; init; }
}

public class CryptoTransaction
{
    public CryptoTransactionType Type { get; init; }
    public DateTime Date { get; init; }
    public double MontantRecu { get; init; }
    public string MonnaieOuJetonRecu { get; init; }
    public double MontantEnvoye { get; init; }
    public string MonnaieOuJetonEnvoye { get; init; }
    public double Frais { get; init; }
    public string MonnaieOuJetonDesFrais { get; init; }
    public string ExchangePlateforme { get; init; }
    public string Description { get; init; }
    public string Label { get; init; }
    public double PrixDuJetonDuMontantEnvoye { get; init; }
    public double PrixDuJetonDuMontantRecu { get; init; }
    public double PrixDuJetonDesFrais { get; init; }
}

public enum CryptoTransactionType
{
    Depot,
    Echange,
    Retrait,
}

public class CryptoAsset
{
    public string Jeton { get; init; }
    public double MontantEnEuros { get; set; }
    public double Montant { get; set; }
    public double Frais { get; set; }

    public override string ToString()
    {
        return $"{Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)}: {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Frais: {Math.Round(Frais, 10, MidpointRounding.ToEven)}";
    }
}

public class Retrait
{
    public DateTime Date { get; init; }
    public string Jeton { get; init; }
    public double Montant { get; init; }
    public double MontantEnEuros { get; init; }
    public double Gains { get; init; }
    public double GainsEnEuros { get; init; }
    public double PrixDuJetonDuMontant { get; init; }
    public double Frais { get; init; }
    public double FraisEnEuros { get; init; }
    public double ValeurGlobale { get; init; }
    public double PrixAcquisition { get; init; }

    public override string ToString()
    {
        return $"{Date} {Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)} {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Gains: {Math.Round(GainsEnEuros, 2, MidpointRounding.ToEven)}€, Prix: {PrixDuJetonDuMontant}, Frais: {Math.Round(FraisEnEuros, 2, MidpointRounding.ToEven)}€, Valeur Global: {Math.Round(ValeurGlobale, 2, MidpointRounding.ToEven)}€, Prix d'acquisition: {Math.Round(PrixAcquisition, 2, MidpointRounding.ToEven)}€";
    }
}