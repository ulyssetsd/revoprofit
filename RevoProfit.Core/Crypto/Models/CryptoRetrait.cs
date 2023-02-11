namespace RevoProfit.Core.Crypto.Models;

public record CryptoRetrait
{
    public required DateTime Date { get; init; }
    public required string Jeton { get; init; }
    public required decimal Montant { get; init; }
    public required decimal MontantEnEuros { get; init; }
    public required decimal GainsEnEuros { get; init; }
    public required decimal PrixDuJeton { get; init; }
    public required decimal Frais { get; init; }
    public required decimal FraisEnEuros { get; init; }

    public override string ToString()
    {
        return $"{Date} {Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)} {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Gains: {Math.Round(GainsEnEuros, 2, MidpointRounding.ToEven)}€, Prix: {PrixDuJeton}, Frais: {Math.Round(FraisEnEuros, 2, MidpointRounding.ToEven)}€";
    }
}