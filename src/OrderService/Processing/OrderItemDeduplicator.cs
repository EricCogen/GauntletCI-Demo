using OrderService.Domain;

namespace OrderService.Processing;

public sealed class OrderItemDeduplicator
{
    public IReadOnlyList<IReadOnlyList<OrderItem>> GroupBySku(Order order)
    {
        var groups = new List<IReadOnlyList<OrderItem>>();
        foreach (var item in order.Items)
        {
            var matches = order.Items
                .Where(i => i.Sku == item.Sku)
                .ToList();
            groups.Add(matches);
        }
        return groups;
    }
}
