using RevoProfit.Core.Revolut.Models;

namespace RevoProfit.Core.Revolut.Services.Interfaces;

public interface IRevolutCsvService
{
    Task<IEnumerable<RevolutTransaction>> ReadCsv(Stream stream);
}