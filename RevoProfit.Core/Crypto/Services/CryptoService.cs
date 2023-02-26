using FluentAssertions;
using RevoProfit.Core.Crypto.Models;
using RevoProfit.Core.Crypto.Services.Interfaces;

namespace RevoProfit.Core.Crypto.Services;

public class CryptoService : ICryptoService
{
    private const int EuroDecimalsPrecision = 24;

    private static CryptoAsset GetOrCreate(ICollection<CryptoAsset> cryptos, string jeton)
    {
        var crypto = cryptos.FirstOrDefault(crypto => crypto.Jeton == jeton);

        if (crypto == null)
        {
            crypto = new CryptoAsset
            {
                Jeton = jeton,
            };
            cryptos.Add(crypto);
        }

        return crypto;
    }

    private static void GereLesFrais(CryptoTransaction transaction, ICollection<CryptoAsset> cryptos)
    {
        if (transaction.Frais == 0)
        {
            return;
        }

        transaction.Frais.Should().NotBe(0);
        transaction.MonnaieOuJetonDesFrais.Should().NotBeEmpty();

        var cryptoFrais = GetOrCreate(cryptos, transaction.MonnaieOuJetonDesFrais);
        cryptoFrais.Frais += transaction.Frais;
    }

    private static void ReinitialiseSiNul(CryptoAsset crypto)
    {
        if (Math.Round(crypto.Montant, 14, MidpointRounding.ToEven) == 0)
        {
            crypto.Montant = 0;
            crypto.MontantEnEuros = 0;
        }
    }

    public IEnumerable<CryptoReport> MapToReports(IEnumerable<CryptoRetrait> cryptoRetraits)
    {
        return cryptoRetraits
            .GroupBy(retrait => retrait.Date.Year)
            .Select(retraits => new CryptoReport
            {
                Year = retraits.Key, 
                GainsEnEuros = retraits.Sum(retrait => retrait.GainsEnEuros),
            });
    }

    public (IEnumerable<CryptoAsset>, IEnumerable<CryptoRetrait>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions)
    {
        var cryptos = new List<CryptoAsset>();
        var retraits = new List<CryptoRetrait>();

        foreach (var transaction in transactions)
        {
            switch (transaction.Type)
            {
                case CryptoTransactionType.Depot:
                {
                    GereLesFrais(transaction, cryptos);

                    transaction.MontantRecu.Should().NotBe(0);
                    transaction.MonnaieOuJetonRecu.Should().NotBeEmpty();
                    transaction.PrixDuJetonDuMontantRecu.Should().NotBe(0);

                    var cryptoRecu = GetOrCreate(cryptos, transaction.MonnaieOuJetonRecu);
                    cryptoRecu.MontantEnEuros += transaction.PrixDuJetonDuMontantRecu * transaction.MontantRecu;
                    cryptoRecu.Montant += transaction.MontantRecu;
                    break;
                }
                case CryptoTransactionType.Echange:
                {
                    GereLesFrais(transaction, cryptos);

                    transaction.MontantRecu.Should().NotBe(0);
                    transaction.MonnaieOuJetonRecu.Should().NotBeEmpty();
                    transaction.PrixDuJetonDuMontantRecu.Should().NotBe(0);

                    transaction.MontantEnvoye.Should().NotBe(0);
                    transaction.MonnaieOuJetonEnvoye.Should().NotBeEmpty();
                    transaction.PrixDuJetonDuMontantEnvoye.Should().NotBe(0);

                    var cryptoEnvoye = GetOrCreate(cryptos, transaction.MonnaieOuJetonEnvoye);

                    var prixDuJetonMoyen = cryptoEnvoye.MontantEnEuros / cryptoEnvoye.Montant;
                    var ratioInsere = prixDuJetonMoyen / transaction.PrixDuJetonDuMontantEnvoye;
                    var montantInsere = transaction.MontantEnvoye * ratioInsere;
                    var montantInsereEnEuros = transaction.PrixDuJetonDuMontantEnvoye * montantInsere;

                    cryptoEnvoye.Montant -= transaction.MontantEnvoye;
                    cryptoEnvoye.MontantEnEuros -= Math.Round(montantInsereEnEuros, EuroDecimalsPrecision, MidpointRounding.ToEven);
                    ReinitialiseSiNul(cryptoEnvoye);

                    var cryptoRecu = GetOrCreate(cryptos, transaction.MonnaieOuJetonRecu);

                    cryptoRecu.Montant += transaction.MontantRecu;
                    cryptoRecu.MontantEnEuros += Math.Round(montantInsereEnEuros, EuroDecimalsPrecision, MidpointRounding.ToEven);
                    break;
                }
                case CryptoTransactionType.Retrait:
                {
                    GereLesFrais(transaction, cryptos);

                    transaction.MontantEnvoye.Should().NotBe(0);
                    transaction.MonnaieOuJetonEnvoye.Should().NotBeEmpty();
                    transaction.PrixDuJetonDuMontantEnvoye.Should().NotBe(0);

                    var cryptoEnvoye = GetOrCreate(cryptos, transaction.MonnaieOuJetonEnvoye);

                    var prixDuJetonMoyen = cryptoEnvoye.MontantEnEuros / cryptoEnvoye.Montant;
                    var ratioInsere = prixDuJetonMoyen / transaction.PrixDuJetonDuMontantEnvoye;
                    var ratioGains = 1 - ratioInsere;

                    var gains = transaction.MontantEnvoye * ratioGains;
                    var gainsEnEuros = gains * transaction.PrixDuJetonDuMontantEnvoye;
                    var montantEnEuros = transaction.MontantEnvoye * transaction.PrixDuJetonDuMontantEnvoye;

                    retraits.Add(new CryptoRetrait
                    {
                        Date = transaction.Date,
                        Jeton = transaction.MonnaieOuJetonEnvoye,
                        Montant = transaction.MontantEnvoye,
                        MontantEnEuros = Math.Round(montantEnEuros, EuroDecimalsPrecision, MidpointRounding.ToEven),
                        GainsEnEuros = Math.Round(gainsEnEuros, EuroDecimalsPrecision, MidpointRounding.ToEven),
                        PrixDuJeton = transaction.PrixDuJetonDuMontantEnvoye,
                    });

                    cryptoEnvoye.Montant -= transaction.MontantEnvoye;
                    cryptoEnvoye.MontantEnEuros -= Math.Round(montantEnEuros * ratioInsere, EuroDecimalsPrecision, MidpointRounding.ToEven);
                    ReinitialiseSiNul(cryptoEnvoye);
                    break;
                }
                case CryptoTransactionType.FeesOnly:
                    GereLesFrais(transaction, cryptos);
                    break;
            }
        }

        return (cryptos, retraits.OrderBy(retrait => retrait.Date));
    }
}