namespace RevoProfit.Core.Crypto.Models;

public class CryptoRetrait
{
    public DateTime Date { get; init; }
    public string Jeton { get; init; }
    public double Montant { get; init; }
    public double MontantEnEuros { get; init; }
    public double GainsEnEuros { get; init; }
    public double PrixDuJeton { get; init; }
    public double Frais { get; init; }
    public double FraisEnEuros { get; init; }

    public override string ToString()
    {
        return $"{Date} {Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)} {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Gains: {Math.Round(GainsEnEuros, 2, MidpointRounding.ToEven)}€, Prix: {PrixDuJeton}, Frais: {Math.Round(FraisEnEuros, 2, MidpointRounding.ToEven)}€";
    }
}