using OrderService.Domain;

namespace OrderService.Pricing;

public sealed class TaxCalculator
{
    private readonly decimal _rate;

    public TaxCalculator(decimal rate = 0.0875m)
    {
        if (rate < 0m) throw new ArgumentOutOfRangeException(nameof(rate));
        _rate = rate;
    }

    public Money TaxFor(Money subtotal) => subtotal.Multiply(_rate);
}
