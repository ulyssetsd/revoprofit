namespace RevoProfit.Core.Stock.Models;

public enum StockTransactionType
{
    Buy,
    Sell,
    CashTopUp,
    CashWithdrawal, 
    CustodyFee,
    CustodyFeeReversal,
    Dividend,
    StockSplit,
}