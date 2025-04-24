using RevoProfit.Core.Revolut2025.Models;

namespace RevoProfit.Core.Revolut2025.Services.Interfaces;

public interface IRevolut2025TransactionMapper
{
    Revolut2025Transaction Map(Revolut2025TransactionCsvLine source);
}
