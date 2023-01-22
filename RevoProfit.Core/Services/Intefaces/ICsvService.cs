namespace RevoProfit.Core.Services.Intefaces;

public interface ICsvService<T>
{
    Task<IEnumerable<T>> ReadCsv(string path);
    Task<IEnumerable<T>> ReadCsv(Stream stream);
}