using OrderService.Abstractions;

namespace OrderService.Pricing;

public class PricingCalculator
{
    private const decimal MaxTransactionAmount = 1_000_000m;

    public decimal ConvertCurrency(double sourceAmount, decimal exchangeRate)
    {
        var converted = (decimal)sourceAmount * exchangeRate;
        if (converted > MaxTransactionAmount)
            throw new OverflowException("Amount exceeds maximum allowed transaction");
        if (converted < 0)
            throw new ArgumentException("Amount cannot be negative");
        return converted;
    }

    public decimal CalculateDiscount(decimal basePrice, int discountPercent)
    {
        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Discount must be between 0 and 100");
        return basePrice * (1 - (discountPercent / 100m));
    }
}
