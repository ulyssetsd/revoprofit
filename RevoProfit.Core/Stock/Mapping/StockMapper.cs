using AutoMapper;
using RevoProfit.Core.Stock.Models;
using System.Text.RegularExpressions;

namespace RevoProfit.Core.Stock.Mapping;

public static class StockMapper
{
    public static void CreateMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<string, TransactionType>().ConvertUsing(MappingFunction);
        cfg.CreateMap<TransactionCsvLine, Transaction>();
    }

    private static TransactionType MappingFunction(string arg1, TransactionType arg2)
    {
        var names = Enum.GetNames<TransactionType>();
        var matchedName = names.FirstOrDefault(name => Flatten(name).Equals(Flatten(arg1)));

        return matchedName != null ? Enum.Parse<TransactionType>(matchedName) : TransactionType.Unknown;
    }

    private static string Flatten(string s) =>
        Regex.Replace(s.ToLower().Replace(" ", string.Empty), @"[^0-9A-Za-z ,]", string.Empty);
}