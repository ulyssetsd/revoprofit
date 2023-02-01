using AutoMapper;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Mapping;
using RevoProfit.Core.Services;
using System.Globalization;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoCsvService : CsvGenericService<CryptoTransaction, CryptoTransactionCsvLine>, ICryptoCsvService
{
    private readonly Mapper _mapper;

    public CryptoCsvService()
    {
        _mapper = MapperFactory.GetMapper();
    }

    public override CryptoTransaction MapCsvLineToModel(CryptoTransactionCsvLine source)
    {
        var lastCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            return _mapper.Map<CryptoTransaction>(source);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }
    }
}