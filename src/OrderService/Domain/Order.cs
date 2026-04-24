namespace OrderService.Domain;

public sealed class Order
{
    public Guid Id { get; }
    public Customer Customer { get; }
    public IReadOnlyList<OrderItem> Items { get; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string Currency { get; }

    public Order(Guid id, Customer customer, IReadOnlyList<OrderItem> items, DateTimeOffset createdAt)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("Order requires at least one item.", nameof(items));
        }

        var currencies = items.Select(i => i.UnitPrice.Currency)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (currencies.Length != 1)
        {
            throw new ArgumentException("All items must share a single currency.", nameof(items));
        }

        Id = id;
        Customer = customer;
        Items = items;
        CreatedAt = createdAt;
        Currency = currencies[0];
        Status = OrderStatus.Pending;
    }

    public Money Subtotal()
        => Items.Aggregate(Money.Zero(Currency), (acc, it) => acc.Add(it.LineTotal));

    public void MarkConfirmed() => Status = OrderStatus.Confirmed;

    public void MarkPaid(DateTimeOffset at)
    {
        Status = OrderStatus.Paid;
        PaidAt = at;
    }

    public void MarkFailed() => Status = OrderStatus.Failed;
    public void MarkCancelled() => Status = OrderStatus.Cancelled;
}
