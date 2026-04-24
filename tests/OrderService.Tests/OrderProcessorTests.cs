using Microsoft.Extensions.Logging.Abstractions;
using OrderService;
using OrderService.Models;
using Xunit;

namespace OrderService.Tests;

public class OrderProcessorTests
{
    private static OrderProcessor MakeProcessor(PaymentClient payments)
        => new(payments, NullLogger<OrderProcessor>.Instance);

    private static PaymentClient MakePayments()
        => new(new HttpClient(), NullLogger<PaymentClient>.Instance, "https://x.test", "k");

    [Fact]
    public async Task Rejects_when_amount_is_zero()
    {
        var processor = MakeProcessor(MakePayments());
        var order = new Order("o1", "c1", 0m, "USD");

        var result = await processor.ProcessAsync(order);

        Assert.False(result.Success);
        Assert.Equal("Amount must be positive", result.Reason);
    }

    [Fact]
    public async Task Rejects_when_amount_is_negative()
    {
        var processor = MakeProcessor(MakePayments());
        var order = new Order("o2", "c1", -1m, "USD");

        var result = await processor.ProcessAsync(order);

        Assert.False(result.Success);
    }
}
