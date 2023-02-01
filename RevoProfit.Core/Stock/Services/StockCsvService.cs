using RevoProfit.Core.Mapping;
using RevoProfit.Core.Services;
using RevoProfit.Core.Stock.Models;
using RevoProfit.Core.Stock.Services.Interfaces;
using System.Globalization;
using AutoMapper;

namespace RevoProfit.Core.Stock.Services;

public class StockCsvService : CsvGenericService<Transaction, TransactionCsvLine>, IStockCsvService
{
    private readonly Mapper _mapper;

    public StockCsvService() : base(CultureInfo.GetCultureInfo("en-GB"))
    {
        _mapper = MapperFactory.GetMapper();
    }

    public override Transaction MapCsvLineToModel(TransactionCsvLine source)
    {
        return _mapper.Map<Transaction>(source);
    }
}
