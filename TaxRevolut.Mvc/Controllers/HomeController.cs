﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaxRevolut.Core.Services.Interfaces;
using TaxRevolut.Mvc.Models;

namespace TaxRevolut.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICsvService _csvService;
        private readonly ITransactionService _transactionService;

        public HomeController(ILogger<HomeController> logger, ICsvService csvService, ITransactionService transactionService)
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
        public ActionResult Result()
        {
            if (ModelState.IsValid)
            {
                var file = Request.Form.Files["formFile"];

                try
                {
                    var transactions = _csvService.ReadCsv(file.OpenReadStream());
                    var annualReports = _transactionService.GetAnnualReports(transactions);

                    return View(new ResultViewModel
                    {
                        AnnualReports = annualReports.ToList(),
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                    throw;
                }
            }

            return View();
        }
    }
}