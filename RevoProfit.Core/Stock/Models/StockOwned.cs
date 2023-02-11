﻿namespace RevoProfit.Core.Stock.Models;

public class StockOwned
{
    public required string Ticker { get; init; }
    public double Quantity { get; set; }
    public double AveragePrice { get; set; }
    public double ValueInserted { get; set; }
    public double TotalDividend { get; set; }
    public override string ToString()
    {
        return $"{Ticker}, Quantity: {Math.Round(Quantity, 14, MidpointRounding.ToEven)}, AveragePrice: ${Math.Round(AveragePrice, 2, MidpointRounding.ToEven)}, ValueInserted: ${Math.Round(ValueInserted, 2, MidpointRounding.ToEven)}, TotalDividend: ${Math.Round(TotalDividend, 2, MidpointRounding.ToEven)}";
    }
}