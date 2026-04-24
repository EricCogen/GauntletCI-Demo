using OrderService.Domain;
using OrderService.Pricing;
using Xunit;

namespace OrderService.Tests.Pricing;

public class TaxCalculatorTests
{
    [Fact]
    public void Computes_tax_at_configured_rate()
    {
        var calc = new TaxCalculator(0.10m);
        var tax = calc.TaxFor(new Money(100m, "USD"));
        Assert.Equal(10m, tax.Amount);
        Assert.Equal("USD", tax.Currency);
    }

    [Fact]
    public void Negative_rate_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TaxCalculator(-0.01m));
    }
}
