using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RevoProfit.Core.Crypto.Services;
using RevoProfit.Core.CurrencyRate.Models;
using RevoProfit.Core.CurrencyRate.Services;
using RevoProfit.Core.Revolut2025.Services;

namespace RevoProfit.Test.Revolut2025;

public class Revolut2025ServiceEndToEndTest
{
    private Revolut2025Service _revolut2025Service = null!;
    private Revolut2025CsvService _revolut2025CsvService = null!;
    private EuropeanCentralBankExchangeRateProvider _exchangeRateProvider = null!;

    [SetUp]
    public async Task Setup()
    {
        _exchangeRateProvider = new EuropeanCentralBankExchangeRateProvider(EuropeanCentralBankUrl.Default);
        _revolut2025CsvService = new Revolut2025CsvService(new Revolut2025TransactionMapper());
        _revolut2025Service = new Revolut2025Service(new CryptoService(new CryptoTransactionFluentValidator(), new CurrencyRateService(_exchangeRateProvider)), new CurrencyRateService(_exchangeRateProvider));
        await _exchangeRateProvider.InitializeAsync();
    }

    [Test]
    public async Task Read_csv_with_a_massive_input_2025_format_should_not_throw_any_exception()
    {
        // Arrange & Act
        var act = async () =>
        {
            await using var memoryStream = new FileStream("../../../../.csv/crypto_input_revolut_2025.csv", FileMode.Open);
            var transactions = await _revolut2025CsvService.ReadCsv(memoryStream);
            _revolut2025Service.ProcessTransactions(transactions);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
    
    [Test]
    public async Task Read_csv_with_a_payment_refund_flagged_as_other_should_not_throw_any_exception_with_content()
    {
        // Arrange & Act
        var act = async () =>
        {
            var content =
            """
            Symbol,Type,Quantity,Price,Value,Fees,Date
            BTC,Buy,0.01713112,"€5,837.33",€100.00,€0.00,"Jun 12, 2018, 4:16:32 PM"
            BTC,Payment,0.00893541,"$7,252.05",$64.80,$0.00,"Jul 20, 2018, 7:28:14 AM"
            BTC,Sell,0.00819571,"€5,504.07",€45.10,€0.00,"Aug 19, 2018, 10:43:55 PM"
            BTC,Other,0.00893541,"$7,252.05",$64.80,$0.00,"Aug 21, 2018, 9:20:13 PM"
            BTC,Sell,0.00879132,"€5,687.43",€50.00,€0.00,"Aug 27, 2018, 10:09:18 AM"
            """;
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var transactions = await _revolut2025CsvService.ReadCsv(memoryStream);
            _revolut2025Service.ProcessTransactions(transactions);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
}