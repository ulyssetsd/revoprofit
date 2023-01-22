using System.Globalization;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;
using RevoProfit.Core.Services;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoCsvService : CsvGenericService<CryptoTransaction, CryptoTransactionCsvLine>, ICryptoCsvService
{
    public CryptoCsvService() : base(CultureInfo.GetCultureInfo("fr-FR")) { }
}