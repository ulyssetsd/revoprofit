using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaxRevolut.Core.Globals;
using TaxRevolut.Core.Services.Interfaces;
using TaxRevolut.Mvc.Models;

namespace TaxRevolut.Mvc.Controllers
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
        public IActionResult Result()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var file = Request.Form.Files["formFile"];
                    var transactions = _csvService.ReadCsv(file.OpenReadStream());
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

        public IActionResult Donate()
        {
            _logger.LogInformation(LogEvents.DonateButtonClicked, "Donate button was clicked");
            return Ok();
        }
    }
}