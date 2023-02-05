using RevoProfit.Core.Revolut.Models;

namespace RevoProfit.Core.Revolut.Services.Interfaces;

public interface IRevolutTransactionMapper
{
    RevolutTransaction Map(RevolutTransactionCsvLine source);
}