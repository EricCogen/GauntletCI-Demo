using OrderService.Domain;
using OrderService.Pricing;
using Xunit;

namespace OrderService.Tests.Pricing;

public class DiscountPolicyTests
{
    [Theory]
    [InlineData(50, 0)]
    [InlineData(100, 5)]
    [InlineData(499, 24.95)]
    [InlineData(500, 50)]
    public void Discount_tiers(decimal subtotal, decimal expected)
    {
        var policy = new DiscountPolicy();
        var d = policy.DiscountFor(new Money(subtotal, "USD"));
        Assert.Equal(expected, d.Amount);
    }
}
