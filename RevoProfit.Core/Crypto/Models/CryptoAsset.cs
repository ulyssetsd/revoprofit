namespace RevoProfit.Core.Crypto.Models;

public class CryptoAsset
{
    public required string Symbol { get; init; }
    public decimal AmountInEuros { get; set; }
    public decimal Amount { get; set; }
    public decimal Fees { get; set; }
}