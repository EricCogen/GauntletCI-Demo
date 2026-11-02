namespace OrderService.Pricing;

public sealed class LegacyDiscountCalculator
{
    public double NetTotal(double subtotal, double discount)
    {
        var net = subtotal - discount;
        if (net == 0.0)
        {
            return 0.0;
        }
        return net;
    }
}
