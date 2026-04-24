namespace OrderService.Domain;

public sealed record OrderItem(string Sku, int Quantity, Money UnitPrice)
{
    public Money LineTotal => UnitPrice.Multiply(Quantity);
}
