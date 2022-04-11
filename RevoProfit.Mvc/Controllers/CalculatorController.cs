using Microsoft.AspNetCore.Mvc;
using RevoProfit.Core.Globals;
using RevoProfit.Core.Services.Interfaces;
using RevoProfit.Mvc.Models;
using System.Diagnostics;

namespace RevoProfit.Mvc.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ILogger<CalculatorController> _logger;
        private readonly ICsvService _csvService;
        private readonly ITransactionService _transactionService;

        public CalculatorController(ILogger<CalculatorController> logger, ICsvService csvService, ITransactionService transactionService)
        {
            _logger = logger;
            _csvService = csvService;
            _transactionService = transactionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Result()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var file = Request.Form.Files["formFile"];
                    var transactions = await _csvService.ReadCsv(file.OpenReadStream());
                    var annualReports = _transactionService.GetAnnualReports(transactions);
                    _logger.LogInformation(LogEvents.GenerateAnnualReports, "Generate annual reports");

                    return View(new ResultViewModel
                    {
                        AnnualReports = annualReports.ToList(),
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Issue while generating the annual reports");
                    throw;
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Donate()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var file = Request.Form.Files["formFile"];
                    var transactions = await _csvService.ReadCsv(file.OpenReadStream());
                    var annualReports = _transactionService.GetAnnualReports(transactions);
                    _logger.LogInformation(LogEvents.GenerateAnnualReportsWithDonation, "Generate annual reports with donation");

                    return View(new ResultViewModel
                    {
                        AnnualReports = annualReports.ToList(),
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Issue while generating the annual reports with donation");
                    throw;
                }
            }

            return View();
        }
    }
}