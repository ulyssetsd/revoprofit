using CsvHelper;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using System.Globalization;
using RevoProfit.Core.Extensions;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoCsvService : ICryptoCsvService
{
    private readonly ICryptoTransactionMapper _cryptoTransactionMapper;

    public CryptoCsvService(ICryptoTransactionMapper cryptoTransactionMapper)
    {
        _cryptoTransactionMapper = cryptoTransactionMapper;
    }

    public async Task<IEnumerable<CryptoTransaction>> ReadCsv(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var csvLines = await csv.GetRecordsAsync<CryptoTransactionCsvLine>().ToEnumerableAsync();
        return csvLines.Select(_cryptoTransactionMapper.Map);
    }
}