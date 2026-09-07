using OrderService.Abstractions;

namespace OrderService.Pricing;

public class PricingCalculator
{
    public decimal ConvertCurrency(double sourceAmount, decimal exchangeRate)
    {
        return (decimal)sourceAmount * exchangeRate;
    }

    public decimal CalculateDiscount(decimal basePrice, int discountPercent)
    {
        return basePrice * (1 - (discountPercent / 100m));
    }
}
