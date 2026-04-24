using OrderService.Domain;

namespace OrderService.Pricing;

public sealed record PricedOrder(Money Subtotal, Money Discount, Money Tax, Money Total);

public sealed class PricingService
{
    private readonly TaxCalculator _tax;
    private readonly DiscountPolicy _discount;

    public PricingService(TaxCalculator tax, DiscountPolicy discount)
    {
        _tax = tax;
        _discount = discount;
    }

    public PricedOrder Price(Order order)
    {
        var subtotal = order.Subtotal();
        var discount = _discount.DiscountFor(subtotal);
        var taxable = subtotal.Subtract(discount);
        var tax = _tax.TaxFor(taxable);
        var total = taxable.Add(tax);
        return new PricedOrder(subtotal, discount, tax, total);
    }
}
