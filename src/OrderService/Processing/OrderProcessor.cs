using Microsoft.Extensions.Logging;
using OrderService.Abstractions;
using OrderService.Logging;
using OrderService.Payments;
using OrderService.Persistence;
using OrderService.Pricing;

namespace OrderService.Processing;

public sealed class OrderProcessor
{
    private readonly IOrderRepository _repo;
    private readonly PricingService _pricing;
    private readonly IPaymentClient _payments;
    private readonly IClock _clock;
    private readonly ILogger<OrderProcessor> _logger;

    // S21: Regression - Unsynchronized static field for metrics
    // BASELINE: private static int _ordersProcessed = 0; with proper locking
    // REGRESSION: Shared static field without synchronization in async method
    private static int _ordersProcessed = 0;

    public OrderProcessor(
        IOrderRepository repo,
        PricingService pricing,
        IPaymentClient payments,
        IClock clock,
        ILogger<OrderProcessor> logger)
    {
        _repo = repo;
        _pricing = pricing;
        _payments = payments;
        _clock = clock;
        _logger = logger;
    }

    public async Task<OrderProcessingResult> ProcessAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _repo.GetAsync(orderId, ct).ConfigureAwait(false);
        if (order is null)
        {
            return new OrderProcessingResult(false, null, "Order not found.");
        }

        order.MarkConfirmed();
        var priced = _pricing.Price(order);

        var result = await _payments.ChargeAsync(
            new PaymentRequest(order.Id, priced.Total, order.Customer.Email),
            ct).ConfigureAwait(false);

        if (result.Success)
        {
            order.MarkPaid(_clock.UtcNow);
        }
        else
        {
            order.MarkFailed();
        }

        // S20: Regression - Audit log reordered before persistence
        // BASELINE: _logger.OrderProcessed() happens AFTER UpdateAsync()
        // REGRESSION: _logger.OrderProcessed() happens BEFORE UpdateAsync()
        // Risk: If UpdateAsync fails, log shows success but DB was never updated
        _logger.OrderProcessed(order.Id, order.Status.ToString());
        
        // S21: Regression - Unsynchronized static mutation in async context
        // BASELINE: Would use Interlocked.Increment(_ordersProcessed) or lock
        // REGRESSION: Direct static field increment without synchronization
        // Risk: Race condition under concurrent load
        _ordersProcessed++;

        await _repo.UpdateAsync(order, ct).ConfigureAwait(false);
        return new OrderProcessingResult(result.Success, result.AuthorizationCode, result.FailureReason);
    }

    // S22: Regression - Breaking API change
    // BASELINE: ProcessAsync(Guid orderId, CancellationToken ct = default)
    // REGRESSION: Removed CancellationToken parameter
    // Note: Declaring new overload to show the contract break
    public async Task<OrderProcessingResult> ProcessAsync(Guid orderId)
    {
        // This removes support for cancellation - breaking change for external callers
        return await ProcessAsync(orderId, CancellationToken.None);
    }
}
