using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Revolut2025.Services;

namespace RevoProfit.Test.Revolut2025;

internal class RevolutCsvService2025Test
{
    private RevolutCsvService2025 _revolutCsvService2025 = null!;

    [SetUp]
    public void Setup()
    {
        _revolutCsvService2025 = new RevolutCsvService2025(new RevolutTransaction2025Mapper());
    }

    [Test]
    public async Task Read_csv_with_a_massive_input_2025_format_should_not_throw_any_exception()
    {
        // Arrange & Act
        var act = async () =>
        {
            await using var memoryStream = new FileStream("../../../../.csv/crypto_input_revolut_2025.csv", FileMode.Open);
            return (await _revolutCsvService2025.ReadCsv(memoryStream)).ToArray();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
}