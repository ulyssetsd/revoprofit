namespace RevoProfit.Core.Crypto.Models;

public class CryptoAsset
{
    public required string Jeton { get; init; }
    public decimal MontantEnEuros { get; set; }
    public decimal Montant { get; set; }
    public decimal Frais { get; set; }

    public override string ToString()
    {
        return $"{Jeton}: {Math.Round(Montant, 10, MidpointRounding.ToEven)}: {Math.Round(MontantEnEuros, 2, MidpointRounding.ToEven)}€, Frais: {Math.Round(Frais, 10, MidpointRounding.ToEven)}";
    }
}