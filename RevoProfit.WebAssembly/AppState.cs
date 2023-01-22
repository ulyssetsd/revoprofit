using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Stock.Models;

namespace RevoProfit.WebAssembly;

public class AppState
{
    private bool _isCalculatingStock;
    private bool _isCalculatingCrypto;
    private bool _hasInsertEmail;

    public bool IsCalculatingStock
    {
        get => _isCalculatingStock;
        set
        {
            _isCalculatingStock = value;
            NotifyStateChanged();
        }
    }

    public bool IsCalculatingCrypto
    {
        get => _isCalculatingCrypto;
        set
        {
            _isCalculatingCrypto = value;
            NotifyStateChanged();
        }
    }

    public bool HasInsertEmail
    {
        get => _hasInsertEmail;
        set
        {
            _hasInsertEmail = value;
            NotifyStateChanged();
        }
    }

    public List<AnnualReport> AnnualReports { get; } = new();
    public List<CryptoAsset> CryptoAssets { get; } = new();
    public List<CryptoRetrait> CryptoRetraits { get; } = new();

    public event Action? OnChange;

    public void SetAnnualReports(IEnumerable<AnnualReport>? annualReports)
    {
        AnnualReports.Clear();
        if (annualReports != null) AnnualReports.AddRange(annualReports);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void SetCryptoAssets(List<CryptoAsset>? cryptoAssets)
    {
        CryptoAssets.Clear();
        if (cryptoAssets != null) CryptoAssets.AddRange(cryptoAssets);
        NotifyStateChanged();
    }

    public void SetCryptoRetraits(List<CryptoRetrait>? cryptoRetraits)
    {
        CryptoRetraits.Clear();
        if (cryptoRetraits != null) CryptoRetraits.AddRange(cryptoRetraits);
        NotifyStateChanged();
    }
}