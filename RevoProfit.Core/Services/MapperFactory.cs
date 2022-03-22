using System.Text.RegularExpressions;
using AutoMapper;
using RevoProfit.Core.Models;

namespace RevoProfit.Core.Services;

public static class MapperFactory
{
    public static Mapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<string, TransactionType>().ConvertUsing(MappingFunction);
            cfg.CreateMap<string, double>().ConvertUsing(MappingFunction);
            cfg.CreateMap<CsvLine, Transaction>();
        });
        return new Mapper(config);
    }

    private static double MappingFunction(string arg1, double arg2)
    {
        return !double.TryParse(arg1, out var result) ? 0 : result;
    }

    private static TransactionType MappingFunction(string arg1, TransactionType arg2)
    {
        var names = Enum.GetNames<TransactionType>();
        var matchedName = names.FirstOrDefault(name => Flatten(name).Equals(Flatten(arg1)));

        return matchedName != null ? Enum.Parse<TransactionType>(matchedName) : TransactionType.Unknown;
    }

    private static string Flatten(string s) => Regex.Replace(s.ToLower().Replace(" ", string.Empty), @"[^0-9A-Za-z ,]", string.Empty);
}