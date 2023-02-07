namespace RevoProfit.Core.Crypto.Models;

public class CryptoRetrait
{
    public DateTime Date { get; init; }
    public string Jeton { get; init; }
    public decimal Montant { get; init; }
    public decimal MontantEnEuros { get; init; }
    public decimal GainsEnEuros { get; init; }
    public decimal PrixDuJeton { get; init; }
    public decimal Frais { get; init; }
    public decimal FraisEnEuros { get; init; }

    public override string ToString()
    {
        return $"{Date} {Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)} {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Gains: {Math.Round(GainsEnEuros, 2, MidpointRounding.ToEven)}€, Prix: {PrixDuJeton}, Frais: {Math.Round(FraisEnEuros, 2, MidpointRounding.ToEven)}€";
    }
}