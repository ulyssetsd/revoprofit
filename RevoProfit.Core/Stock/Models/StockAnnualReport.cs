using RevoProfit.Core.Stock.Services;

namespace RevoProfit.Core.Stock.Models;

public record StockAnnualReport
{
    public required int Year { get; init; }
    public required decimal Dividends { get; init; }
    public required decimal CashTopUp { get; init; }
    public required decimal CashWithdrawal { get; init; }
    public required decimal CustodyFee { get; init; }

    public required decimal DividendsInEuro { get; init; }
    public required decimal CashTopUpInEuro { get; init; }
    public required decimal CashWithdrawalInEuro { get; init; }
    public required decimal CustodyFeeInEuro { get; init; }

    public required StockSellAnnualReport SellReport { get; init; }
}