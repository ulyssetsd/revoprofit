namespace RevoProfit.Core.Crypto.Models;

public record CryptoTransaction
{
    public required CryptoTransactionType Type { get; init; }
    public required DateTime Date { get; init; }
    public required decimal MontantRecu { get; init; }
    public required string MonnaieOuJetonRecu { get; init; }
    public required decimal PrixDuJetonDuMontantRecu { get; init; }
    public required decimal MontantEnvoye { get; init; }
    public required string MonnaieOuJetonEnvoye { get; init; }
    public required decimal PrixDuJetonDuMontantEnvoye { get; init; }
    public required decimal Frais { get; init; }
    public required string MonnaieOuJetonDesFrais { get; init; }
    public required decimal PrixDuJetonDesFrais { get; init; }
}