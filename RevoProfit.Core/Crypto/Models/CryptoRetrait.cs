namespace RevoProfit.Core.Crypto.Models;

public record CryptoRetrait
{
    public required DateTime Date { get; init; }
    public required string Jeton { get; init; }
    public required decimal Montant { get; init; }
    public required decimal MontantEnEuros { get; init; }
    public required decimal GainsEnEuros { get; init; }
    public required decimal PrixDuJeton { get; init; }
}