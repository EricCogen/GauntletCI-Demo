using OrderService.Domain;
using OrderService.Persistence;
using Xunit;

namespace OrderService.Tests.Persistence;

public class InMemoryOrderRepositoryTests
{
    private static Order MakeOrder() => new(
        Guid.NewGuid(),
        new Customer(Guid.NewGuid(), "a@b.com", "Test User"),
        new[] { new OrderItem("SKU-1", 2, new Money(50m, "USD")) },
        DateTimeOffset.UtcNow);

    [Fact]
    public async Task Add_then_Get_returns_same_order()
    {
        var repo = new InMemoryOrderRepository();
        var o = MakeOrder();
        await repo.AddAsync(o);
        var fetched = await repo.GetAsync(o.Id);
        Assert.Same(o, fetched);
    }

    [Fact]
    public async Task Add_duplicate_throws()
    {
        var repo = new InMemoryOrderRepository();
        var o = MakeOrder();
        await repo.AddAsync(o);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(o));
    }

    [Fact]
    public async Task List_returns_all()
    {
        var repo = new InMemoryOrderRepository();
        await repo.AddAsync(MakeOrder());
        await repo.AddAsync(MakeOrder());
        var list = await repo.ListAsync();
        Assert.Equal(2, list.Count);
    }
}
