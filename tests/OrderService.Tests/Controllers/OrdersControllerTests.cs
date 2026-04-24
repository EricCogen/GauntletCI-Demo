using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using OrderService.Controllers;
using OrderService.Persistence;
using OrderService.Pricing;
using OrderService.Processing;
using OrderService.Tests.Fakes;
using Xunit;

namespace OrderService.Tests.Controllers;

public class OrdersControllerTests
{
    private static OrdersController Build(out IOrderRepository repo)
    {
        var inMemory = new InMemoryOrderRepository();
        repo = inMemory;
        var pricing = new PricingService(new TaxCalculator(0.10m), new DiscountPolicy());
        var processor = new OrderProcessor(
            inMemory, pricing, new FakePaymentClient(), new FixedClock(),
            NullLogger<OrderProcessor>.Instance);
        return new OrdersController(inMemory, processor, new FixedClock());
    }

    [Fact]
    public async Task Create_returns_CreatedAtAction()
    {
        var ctrl = Build(out _);
        var result = await ctrl.Create(
            new CreateOrderRequest(
                Guid.NewGuid(), "a@b.com", "Test", "USD",
                new[] { new CreateOrderItem("SKU-1", 1, 10m) }),
            CancellationToken.None);
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Get_unknown_returns_NotFound()
    {
        var ctrl = Build(out _);
        var result = await ctrl.Get(Guid.NewGuid(), CancellationToken.None);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
