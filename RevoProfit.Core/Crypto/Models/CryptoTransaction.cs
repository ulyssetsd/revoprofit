namespace RevoProfit.Core.Crypto.Models;

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