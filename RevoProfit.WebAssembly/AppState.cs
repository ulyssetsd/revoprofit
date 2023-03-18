using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Stock.Models;

namespace RevoProfit.WebAssembly;

public class AppState
{
    private bool _isCalculatingStock;
    private bool _isCalculatingCrypto;
    private IReadOnlyCollection<StockAnnualReport> _stockAnnualReports = new List<StockAnnualReport>();
    private IReadOnlyCollection<CryptoAsset> _cryptoAssets = new List<CryptoAsset>();
    private IReadOnlyCollection<CryptoSell> _cryptoRetraits = new List<CryptoSell>();
    private IReadOnlyCollection<OwnedStock> _ownedStocks = new List<OwnedStock>();
    private IReadOnlyCollection<CryptoReport> _cryptoReports = new List<CryptoReport>();

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

    public IReadOnlyCollection<StockAnnualReport> StockAnnualReports
    {
        get => _stockAnnualReports;
        set
        {
            _stockAnnualReports = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyCollection<CryptoAsset> CryptoAssets
    {
        get => _cryptoAssets;
        set
        {
            _cryptoAssets = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyCollection<CryptoSell> CryptoRetraits
    {
        get => _cryptoRetraits;
        set
        {
            _cryptoRetraits = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyCollection<CryptoReport> CryptoReports
    {
        get => _cryptoReports;
        set
        {
            _cryptoReports = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyCollection<OwnedStock> OwnedStocks
    {
        get => _ownedStocks;
        set
        {
            _ownedStocks = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}