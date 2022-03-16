namespace TaxRevolut.Models;

public enum Currency
{
    Usd
}

public enum TransactionType
{
    Unknown,
    Buy,
    CashTopUp,
    CustodyFee,
    Dividend,
    Sell,
    StockSplit,
}