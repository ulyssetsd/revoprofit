namespace RevoProfit.Core.Crypto.Models;

public record CryptoTransaction
{
    public CryptoTransactionType Type { get; init; }
    public DateTime Date { get; init; }
    public double MontantRecu { get; init; }
    public string MonnaieOuJetonRecu { get; init; }
    public double PrixDuJetonDuMontantRecu { get; init; }
    public double MontantEnvoye { get; init; }
    public string MonnaieOuJetonEnvoye { get; init; }
    public double PrixDuJetonDuMontantEnvoye { get; init; }
    public double Frais { get; init; }
    public string MonnaieOuJetonDesFrais { get; init; }
    public double PrixDuJetonDesFrais { get; init; }
}