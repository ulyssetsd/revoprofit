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

    private void GereLesFrais(CryptoTransaction transaction, List<CryptoAsset> cryptos)
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

    private void ReinitialiseSiNul(CryptoAsset crypto)
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

            if (transaction.Type == CryptoTransactionType.Depot)
            {
                GereLesFrais(transaction, cryptos);
             
                transaction.MontantRecu.Should().NotBe(0);
                transaction.MonnaieOuJetonRecu.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantRecu.Should().NotBe(0);

                var cryptoRecu = GetOrCreate(cryptos, transaction.MonnaieOuJetonRecu);
                cryptoRecu.MontantEnEuros += transaction.PrixDuJetonDuMontantRecu * transaction.MontantRecu;
                cryptoRecu.Montant += transaction.MontantRecu;
            }

            if (transaction.Type == CryptoTransactionType.Echange)
            {
                GereLesFrais(transaction, cryptos);

                transaction.MontantRecu.Should().NotBe(0);
                transaction.MonnaieOuJetonRecu.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantRecu.Should().NotBe(0);

                transaction.MontantEnvoye.Should().NotBe(0);
                transaction.MonnaieOuJetonEnvoye.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantEnvoye.Should().NotBe(0);

                var cryptoEnvoye = GetOrCreate(cryptos, transaction.MonnaieOuJetonEnvoye);

                var prixDuJetonEnvoyeMoyen = cryptoEnvoye.MontantEnEuros / cryptoEnvoye.Montant;
                var ratioInsereEnvoye = prixDuJetonEnvoyeMoyen / transaction.PrixDuJetonDuMontantEnvoye;
                var montantInsereEnvoye = transaction.MontantEnvoye * ratioInsereEnvoye;
                var montantInsereEnvoyeEnDollars = transaction.PrixDuJetonDuMontantEnvoye * montantInsereEnvoye;

                cryptoEnvoye.Montant -= transaction.MontantEnvoye;
                cryptoEnvoye.MontantEnEuros -= montantInsereEnvoyeEnDollars;
                ReinitialiseSiNul(cryptoEnvoye);

                var cryptoRecu = GetOrCreate(cryptos, transaction.MonnaieOuJetonRecu);

                cryptoRecu.Montant += transaction.MontantRecu;
                cryptoRecu.MontantEnEuros += montantInsereEnvoyeEnDollars;
            }

            if (transaction.Type == CryptoTransactionType.Retrait)
            {
                GereLesFrais(transaction, cryptos);

                transaction.MontantEnvoye.Should().NotBe(0);
                transaction.MonnaieOuJetonEnvoye.Should().NotBeEmpty();
                transaction.PrixDuJetonDuMontantEnvoye.Should().NotBe(0);

                var cryptoEnvoye = GetOrCreate(cryptos, transaction.MonnaieOuJetonEnvoye);
                var prixDuJetonMoyen = cryptoEnvoye.MontantEnEuros / cryptoEnvoye.Montant;
                var ratioInsere = prixDuJetonMoyen / transaction.PrixDuJetonDuMontantEnvoye;
                var ratioPlusValue = 1 - ratioInsere;

                var gains = transaction.MontantEnvoye * ratioPlusValue;
                var gainsEnDollars = gains * transaction.PrixDuJetonDuMontantEnvoye;
                var montantEnvoyeEnDollars = transaction.MontantEnvoye * transaction.PrixDuJetonDuMontantEnvoye;

                retraits.Add(new Retrait
                {
                    Date = transaction.Date,
                    Jeton = transaction.MonnaieOuJetonEnvoye,
                    Montant = transaction.MontantEnvoye,
                    MontantEnEuros = montantEnvoyeEnDollars,
                    Gains = gains,
                    GainsEnEuros = gainsEnDollars,
                    PrixDuJetonDuMontant = transaction.PrixDuJetonDuMontantEnvoye,
                    Frais = transaction.Frais,
                    FraisEnEuros = transaction.Frais * transaction.PrixDuJetonDesFrais,
                    ValeurGlobale = cryptoEnvoye.Montant * transaction.PrixDuJetonDuMontantEnvoye,
                    PrixAcquisition = cryptoEnvoye.MontantEnEuros,
                });

                cryptoEnvoye.Montant -= transaction.MontantEnvoye;
                cryptoEnvoye.MontantEnEuros -= montantEnvoyeEnDollars * ratioInsere;
                ReinitialiseSiNul(cryptoEnvoye);
            }
        }

        return (cryptos, retraits);
    }
}