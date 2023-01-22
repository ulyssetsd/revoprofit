using AutoMapper;
using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Mapping;

public static class CryptoMapper
{
    public static void CreateMap(IMapperConfigurationExpression cfg)
    {
        cfg.CreateMap<string, CryptoTransactionType>().ConvertUsing(MappingFunction);
        cfg.CreateMap<CryptoTransactionCsvLine, CryptoTransaction>();
    }

    private static CryptoTransactionType MappingFunction(string arg1, CryptoTransactionType arg2) => arg1 switch
    {
        "Dépôt" => CryptoTransactionType.Depot,
        "Retrait" => CryptoTransactionType.Retrait,
        "Échange" => CryptoTransactionType.Echange,
        _ => throw new ArgumentOutOfRangeException()
    };
}