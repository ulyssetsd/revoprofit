using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Stock.Models;

namespace RevoProfit.WebAssembly;

public class AppState
{
    private bool _isCalculatingStock;
    private bool _isCalculatingCrypto;
    private IEnumerable<StockAnnualReport> _stockAnnualReports = new List<StockAnnualReport>();
    private IEnumerable<CryptoAsset> _cryptoAssets = new List<CryptoAsset>();
    private IEnumerable<CryptoRetrait> _cryptoRetraits = new List<CryptoRetrait>();
    private IEnumerable<StockOwned> _stockOwneds = new List<StockOwned>();

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

    public IEnumerable<StockAnnualReport> StockAnnualReports
    {
        get => _stockAnnualReports;
        set
        {
            _stockAnnualReports = value;
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

    public IEnumerable<StockOwned> StockOwneds
    {
        get => _stockOwneds;
        set
        {
            _stockOwneds = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}