using System.Collections.Concurrent;
using OrderService.Domain;

namespace OrderService.Persistence;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _store = new();

    public Task<Order?> GetAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(id, out var o) ? o : null);

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        if (!_store.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException($"Order {order.Id} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Order>> ListAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Order>>(_store.Values.ToList());
}
