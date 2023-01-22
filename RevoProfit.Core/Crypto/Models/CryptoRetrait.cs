namespace RevoProfit.Core.Crypto.Models;

public class CryptoRetrait
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