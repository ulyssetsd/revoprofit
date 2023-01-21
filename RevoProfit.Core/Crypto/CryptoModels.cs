using CsvHelper.Configuration.Attributes;

namespace RevoProfit.Core.Crypto;

public class CryptoTransactionCsvLine
{
    [Name("Type")] public string? Type { get; set; }
    [Name("Date")] public string? Date { get; set; }
    [Name("Montant reçu")] public string? MontantRecu { get; set; }
    [Name("Monnaie ou jeton reçu")] public string? MonnaieOuJetonRecu { get; set; }
    [Name("Montant envoyé")] public string? MontantEnvoye { get; set; }
    [Name("Monnaie ou jeton envoyé")] public string? MonnaieOuJetonEnvoye { get; set; }
    [Name("Frais")] public string? Frais { get; set; }
    [Name("Monnaie ou jeton des frais")] public string? MonnaieOuJetonDesFrais { get; set; }
    [Name("Exchange / Plateforme")] public string? ExchangePlateforme { get; set; }
    [Name("Description")] public string? Description { get; set; }
    [Name("Label")] public string? Label { get; set; }
    [Name("Prix du jeton du montant envoyé")] public string? PrixDuJetonDuMontantEnvoye { get; set; }
    [Name("Prix du jeton du montant recu")] public string? PrixDuJetonDuMontantRecu { get; set; }
    [Name("Prix du jeton des frais")] public string? PrixDuJetonDesFrais { get; set; }
}

public class CryptoTransaction
{
    public CryptoTransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public double MontantRecu { get; set; }
    public string MonnaieOuJetonRecu { get; set; }
    public double MontantEnvoye { get; set; }
    public string MonnaieOuJetonEnvoye { get; set; }
    public double Frais { get; set; }
    public string MonnaieOuJetonDesFrais { get; set; }
    public string ExchangePlateforme { get; set; }
    public string Description { get; set; }
    public string Label { get; set; }
    public double PrixDuJetonDuMontantEnvoye { get; set; }
    public double PrixDuJetonDuMontantReçu { get; set; }
    public double PrixDuJetonDesFrais { get; set; }
}

public enum CryptoTransactionType
{
    Depot,
    Echange,
    Retrait,
}

/*
public enum Label
{
    Salaire,
    DépôtDExchangeAyantFermé,
    DonPerçu,
    AirdropFork,
    GainAutreRevenu,
    RevenusDeStackingMasternodesEtMining,
    CFD,
    Cashback,
    OpérationInterne,
    OpérationExterne,
    Paiement,
    HackPerte,
    Donation,
    PerteAutreFrais,
}

public enum LabelPourDépôt
{
    Salaire,
    DépôtDExchangeAyantFermé,
    DonPerçu,
    AirdropFork,
    GainAutreRevenu,
    RevenusDeStackingMasternodesEtMining,
    CFD,
    Cashback,
    OpérationInterne,
    OpérationExterne,
}

public enum LabelPourRetrait
{
    Paiement,
    HackPerte,
    Donation,
    PerteAutreFrais,
    CFD,
    Cashback,
    OpérationInterne,
    OpérationExterne,
}
*/

public class CryptoAsset
{
    public string Jeton { get; set; }
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
    public DateTime Date { get; set; }
    public string Jeton { get; set; }
    public double Montant { get; set; }
    public double MontantEnEuros { get; set; }
    public double Gains { get; set; }
    public double GainsEnEuros { get; set; }
    public double PrixDuJetonDuMontant { get; set; }
    public double Frais { get; set; }
    public double FraisEnEuros { get; set; }
    public double ValeurGlobale { get; set; }
    public double PrixAcquisition { get; set; }

    public override string ToString()
    {
        return $"{Date} {Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)} {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Gains: {Math.Round(GainsEnEuros, 2, MidpointRounding.ToEven)}€, Prix: {PrixDuJetonDuMontant}, Frais: {Math.Round(FraisEnEuros, 2, MidpointRounding.ToEven)}€, Valeur Global: {Math.Round(ValeurGlobale, 2, MidpointRounding.ToEven)}€, Prix d'acquisition: {Math.Round(PrixAcquisition, 2, MidpointRounding.ToEven)}€";
    }
}