using AutoMapper;
using CsvHelper;
using FluentAssertions;
using RevoProfit.Core.Extensions;
using RevoProfit.Core.Mapping;
using System.Globalization;

namespace RevoProfit.Core.Crypto;

public interface ICryptoService
{
    Task<IEnumerable<CryptoTransaction>> ReadCsv(string path);
    Task<IEnumerable<CryptoTransaction>> ReadCsv(Stream stream);
    (List<CryptoAsset>, List<Retrait>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions);
}

public class CryptoService : ICryptoService
{
    private readonly Mapper _mapper;

    public CryptoService()
    {
        _mapper = MapperFactory.GetMapper();
    }

    public async Task<IEnumerable<CryptoTransaction>> ReadCsv(string path)
    {
        using var reader = new StreamReader(path);
        return await ReadCsv(reader);
    }

    public async Task<IEnumerable<CryptoTransaction>> ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await ReadCsv(reader);
    }

    private async Task<IEnumerable<CryptoTransaction>> ReadCsv(TextReader streamReader)
    {
        using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var lastCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            var csvLines = await csv.GetRecordsAsync<CryptoTransactionCsvLine>().ToEnumerableAsync();
            return csvLines.Select(_mapper.Map<CryptoTransaction>).ToList();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }
    }

    private CryptoAsset GetOrCreate(List<CryptoAsset> cryptos, string jeton)
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

    private void GèreLesFrais(CryptoTransaction transaction, List<CryptoAsset> cryptos)
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

    private void RéinitialiseSiNul(CryptoAsset crypto)
    {
        if (Math.Round(crypto.Montant, 14, MidpointRounding.ToEven) == 0)
        {
            crypto.Montant = 0;
            crypto.MontantEnEuros = 0;
        }
    }
    
    public (List<CryptoAsset>, List<Retrait>) ProcessTransactions(IEnumerable<CryptoTransaction> transactions)
    {
        var cryptos = new List<CryptoAsset>();
        var retraits = new List<Retrait>();

        foreach (var transaction in transactions)
        {
            if (transaction == null)
            {
                return (new List<CryptoAsset>(), new List<Retrait>());
            }

            if (transaction.Type == CryptoTransactionType.Dépôt)
            {
                GèreLesFrais(transaction, cryptos);
             
                transaction.MontantReçu.Should().NotBe(0);
                transaction.MonnaieOuJetonReçu.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantReçu.Should().NotBe(0);

                var cryptoReçu = GetOrCreate(cryptos, transaction.MonnaieOuJetonReçu);
                cryptoReçu.MontantEnEuros += transaction.PrixDuJetonDuMontantReçu * transaction.MontantReçu;
                cryptoReçu.Montant += transaction.MontantReçu;
            }

            if (transaction.Type == CryptoTransactionType.Échange)
            {
                GèreLesFrais(transaction, cryptos);

                transaction.MontantReçu.Should().NotBe(0);
                transaction.MonnaieOuJetonReçu.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantReçu.Should().NotBe(0);

                transaction.MontantEnvoyé.Should().NotBe(0);
                transaction.MonnaieOuJetonEnvoyé.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantEnvoyé.Should().NotBe(0);

                var cryptoEnvoyé = GetOrCreate(cryptos, transaction.MonnaieOuJetonEnvoyé);

                var prixDuJetonEnvoyéMoyen = cryptoEnvoyé.MontantEnEuros / cryptoEnvoyé.Montant;
                var ratioInséréEnvoyé = prixDuJetonEnvoyéMoyen / transaction.PrixDuJetonDuMontantEnvoyé;
                var montantInséréEnvoyé = transaction.MontantEnvoyé * ratioInséréEnvoyé;
                var montantInséréEnvoyéEnDollars = transaction.PrixDuJetonDuMontantEnvoyé * montantInséréEnvoyé;

                cryptoEnvoyé.Montant -= transaction.MontantEnvoyé;
                cryptoEnvoyé.MontantEnEuros -= montantInséréEnvoyéEnDollars;
                RéinitialiseSiNul(cryptoEnvoyé);

                var cryptoReçu = GetOrCreate(cryptos, transaction.MonnaieOuJetonReçu);

                cryptoReçu.Montant += transaction.MontantReçu;
                cryptoReçu.MontantEnEuros += montantInséréEnvoyéEnDollars;
            }

            if (transaction.Type == CryptoTransactionType.Retrait)
            {
                transaction.MontantEnvoyé.Should().NotBe(0);
                transaction.MonnaieOuJetonEnvoyé.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantEnvoyé.Should().NotBe(0);

                var cryptoEnvoyé = GetOrCreate(cryptos, transaction.MonnaieOuJetonEnvoyé);
                var prixDuJetonMoyen = cryptoEnvoyé.MontantEnEuros / cryptoEnvoyé.Montant;
                var ratioInséré = prixDuJetonMoyen / transaction.PrixDuJetonDuMontantEnvoyé;
                var ratioPlusValue = 1 - ratioInséré;

                var gains = transaction.MontantEnvoyé * ratioPlusValue;
                var gainsEnDollars = gains * transaction.PrixDuJetonDuMontantEnvoyé;
                var montantEnvoyéEnDollars = transaction.MontantEnvoyé * transaction.PrixDuJetonDuMontantEnvoyé;

                retraits.Add(new Retrait
                {
                    Date = transaction.Date,
                    Jeton = transaction.MonnaieOuJetonEnvoyé,
                    Montant = transaction.MontantEnvoyé,
                    MontantEnEuros = montantEnvoyéEnDollars,
                    Gains = gains,
                    GainsEnEuros = gainsEnDollars,
                    PrixDuJetonDuMontant = transaction.PrixDuJetonDuMontantEnvoyé,
                });

                cryptoEnvoyé.Montant -= transaction.MontantEnvoyé;
                cryptoEnvoyé.MontantEnEuros -= montantEnvoyéEnDollars * ratioInséré;
                RéinitialiseSiNul(cryptoEnvoyé);
            }
        }

        return (cryptos, retraits);
    }
}