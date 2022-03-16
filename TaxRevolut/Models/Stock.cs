namespace TaxRevolut.Models;

public class Stock
{
    public double Quantity { get; internal set; }
    public double AveragePrice { get; internal set; }
    public double ValueInserted { get; internal set; }

    public void AddTransaction(StockTransaction transaction)
    {
        if (transaction.Quantity < 0)
        {
            //TODO
        }
        else
        {
            ValueInserted += transaction.Quantity * transaction.PricePerShare;
            Quantity += transaction.Quantity;
            AveragePrice = Quantity / ValueInserted;
        }
    }
}