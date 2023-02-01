using AutoMapper;
using RevoProfit.Core.Crypto.Mapping;

namespace RevoProfit.Core.Mapping;

public static class MapperFactory
{
    public static Mapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<string, double>().ConvertUsing(MappingFunction);
            CryptoMapper.CreateMap(cfg);
        });
        return new Mapper(config);
    }

    private static double MappingFunction(string arg1, double arg2)
    {
        return !double.TryParse(arg1, out var result) ? 0 : result;
    }
}