using Microsoft.Extensions.Logging.Abstractions;
using OrderService.Domain;
using OrderService.Payments;
using OrderService.Persistence;
using OrderService.Pricing;
using OrderService.Processing;
using OrderService.Tests.Fakes;
using Xunit;

namespace OrderService.Tests.Processing;

public class OrderProcessorTests
{
    private static Order Make(decimal unitPrice, int qty)
        => new(
            Guid.NewGuid(),
            new Customer(Guid.NewGuid(), "test@example.com", "Test"),
            new[] { new OrderItem("SKU-1", qty, new Money(unitPrice, "USD")) },
            DateTimeOffset.UtcNow);

    private static OrderProcessor BuildProcessor(InMemoryOrderRepository repo, FakePaymentClient payments)
    {
        var pricing = new PricingService(new TaxCalculator(0.10m), new DiscountPolicy());
        return new OrderProcessor(repo, pricing, payments, new FixedClock(), NullLogger<OrderProcessor>.Instance);
    }

    [Fact]
    public async Task Successful_payment_marks_order_paid()
    {
        var repo = new InMemoryOrderRepository();
        var order = Make(50m, 2);
        await repo.AddAsync(order);
        var payments = new FakePaymentClient();
        var processor = BuildProcessor(repo, payments);

        var result = await processor.ProcessAsync(order.Id);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Single(payments.Calls);
    }

    [Fact]
    public async Task Failed_payment_marks_order_failed()
    {
        var repo = new InMemoryOrderRepository();
        var order = Make(50m, 2);
        await repo.AddAsync(order);
        var payments = new FakePaymentClient
        {
            Responder = _ => new PaymentResult(false, null, "declined"),
        };
        var processor = BuildProcessor(repo, payments);

        var result = await processor.ProcessAsync(order.Id);

        Assert.False(result.Success);
        Assert.Equal(OrderStatus.Failed, order.Status);
    }

    [Fact]
    public async Task Missing_order_returns_not_found()
    {
        var repo = new InMemoryOrderRepository();
        var processor = BuildProcessor(repo, new FakePaymentClient());

        var result = await processor.ProcessAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Order not found.", result.FailureReason);
    }
}
