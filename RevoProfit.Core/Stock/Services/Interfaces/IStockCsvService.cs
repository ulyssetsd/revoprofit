using RevoProfit.Core.Services.Intefaces;
using RevoProfit.Core.Stock.Models;

namespace RevoProfit.Core.Stock.Services.Interfaces;

public interface IStockCsvService : ICsvService<Transaction> { }