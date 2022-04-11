using RevoProfit.Core.Models;

namespace RevoProfit.WebAssembly;

public class AppState
{
    private bool _isCalculatingAnnualReports;

    public bool IsCalculatingAnnualReports
    {
        get => _isCalculatingAnnualReports;
        set
        {
            _isCalculatingAnnualReports = value;
            NotifyStateChanged();
        }
    }

    private bool _hasInsertEmail;

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

    public event Action? OnChange;

    public void SetAnnualReports(IEnumerable<AnnualReport>? annualReports)
    {
        AnnualReports.Clear();
        if (annualReports != null) AnnualReports.AddRange(annualReports);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}