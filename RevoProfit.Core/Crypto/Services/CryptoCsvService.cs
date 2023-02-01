using System.Globalization;
using AutoMapper;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Mapping;
using RevoProfit.Core.Services;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoCsvService : CsvGenericService<CryptoTransaction, CryptoTransactionCsvLine>, ICryptoCsvService
{
    private readonly Mapper _mapper;

    public CryptoCsvService() : base(CultureInfo.GetCultureInfo("fr-FR"))
    {
        _mapper = MapperFactory.GetMapper();
    }

    public override CryptoTransaction MapCsvLineToModel(CryptoTransactionCsvLine source)
    {
        return _mapper.Map<CryptoTransaction>(source);
    }
}