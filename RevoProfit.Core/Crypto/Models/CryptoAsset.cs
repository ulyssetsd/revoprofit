namespace RevoProfit.Core.Crypto.Models;

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