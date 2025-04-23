using RevoProfit.Core.Revolut2025.Models;

namespace RevoProfit.Core.Revolut2025.Services.Interfaces;

public interface IRevolut2025CsvService
{
    Task<IEnumerable<Revolut2025Transaction>> ReadCsv(Stream stream);
}
