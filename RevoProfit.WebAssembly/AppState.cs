using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Stock.Models;

namespace RevoProfit.WebAssembly;

public class AppState
{
    private bool _isCalculatingStock;
    private bool _isCalculatingCrypto;
    private IEnumerable<StockAnnualReport> _annualReports = new List<StockAnnualReport>();
    private IEnumerable<CryptoAsset> _cryptoAssets = new List<CryptoAsset>();
    private IEnumerable<CryptoRetrait> _cryptoRetraits = new List<CryptoRetrait>();

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

    public IEnumerable<StockAnnualReport> AnnualReports
    {
        get => _annualReports;
        set
        {
            _annualReports = value;
            NotifyStateChanged();
        }
    }

    public IEnumerable<CryptoAsset> CryptoAssets
    {
        get => _cryptoAssets;
        set
        {
            _cryptoAssets = value;
            NotifyStateChanged();
        }
    }

    public IEnumerable<CryptoRetrait> CryptoRetraits
    {
        get => _cryptoRetraits;
        set
        {
            _cryptoRetraits = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}