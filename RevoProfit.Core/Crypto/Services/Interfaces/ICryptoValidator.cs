using RevoProfit.Core.Crypto.Models;

namespace RevoProfit.Core.Crypto.Services.Interfaces
{
    public interface ICryptoValidator
    {
        void Validate(CryptoTransaction transaction);
    }
}
