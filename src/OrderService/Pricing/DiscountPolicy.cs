using OrderService.Domain;

namespace OrderService.Pricing;

public sealed class DiscountPolicy
{
    public Money DiscountFor(Money subtotal)
    {
        if (subtotal.Amount >= 500m)
        {
            return subtotal.Multiply(0.10m);
        }
        if (subtotal.Amount >= 100m)
        {
            return subtotal.Multiply(0.05m);
        }
        return Money.Zero(subtotal.Currency);
    }
}
