namespace RevoProfit.Core.Crypto.Models;

public record CryptoTransaction
{
    public CryptoTransactionType Type { get; init; }
    public DateTime Date { get; init; }
    public decimal MontantRecu { get; init; }
    public string MonnaieOuJetonRecu { get; init; }
    public decimal PrixDuJetonDuMontantRecu { get; init; }
    public decimal MontantEnvoye { get; init; }
    public string MonnaieOuJetonEnvoye { get; init; }
    public decimal PrixDuJetonDuMontantEnvoye { get; init; }
    public decimal Frais { get; init; }
    public string MonnaieOuJetonDesFrais { get; init; }
    public decimal PrixDuJetonDesFrais { get; init; }
}