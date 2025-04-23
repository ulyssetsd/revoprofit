using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.Revolut2025.Services;

namespace RevoProfit.Test.Revolut2025;

public class RevolutService2025EndToEndTest
{
    private RevolutService2025 _revolutService2025 = null!;
    private RevolutCsvService2025 _revolutCsvService2025 = null!;

    [SetUp]
    public void Setup()
    {
        _revolutCsvService2025 = new RevolutCsvService2025(new RevolutTransaction2025Mapper());
        _revolutService2025 = new RevolutService2025(new CryptoService(new CryptoTransactionFluentValidator()));
    }

    [Test]
    public async Task Read_csv_with_a_massive_input_2025_format_should_not_throw_any_exception()
    {
        // Arrange & Act
        var act = async () =>
        {
            await using var memoryStream = new FileStream("../../../../.csv/crypto_input_revolut_2025.csv", FileMode.Open);
            var transactions = await _revolutCsvService2025.ReadCsv(memoryStream);
            _revolutService2025.ProcessTransactions(transactions);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
}