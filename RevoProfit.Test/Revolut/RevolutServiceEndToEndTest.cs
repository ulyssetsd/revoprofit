using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Revolut.Services;

namespace RevoProfit.Test.Revolut;

public class RevolutServiceEndToEndTest
{
    private RevolutService _revolutService = null!;
    private RevolutCsvService _revolutCsvService = null!;

    [SetUp]
    public void Setup()
    {
        _revolutCsvService = new RevolutCsvService(new RevolutTransactionMapper());
        _revolutService = new RevolutService(new CryptoService(new CryptoTransactionFluentValidator(), new EuropeanCentralBankExchangeRateProvider()));
    }

    [Test]
    public async Task Read_csv_with_a_massive_input_should_not_throw_any_exception()
    {
        // Arrange & Act
        var act = async () =>
        {
            await using var memoryStream = new FileStream("../../../../.csv/crypto_input_revolut_2022.csv", FileMode.Open);
            var transactions = await _revolutCsvService.ReadCsv(memoryStream);
            _revolutService.ProcessTransactions(transactions);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
}