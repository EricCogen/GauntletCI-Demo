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

        PaymentResult result;
        try
        {
            result = await _payments.ChargeAsync(
                new PaymentRequest(order.Id, priced.Total, order.Customer.Email),
                ct).ConfigureAwait(false);
        }
        catch
        {
            // Soft-fail: payment provider sometimes throws on transient errors,
            // we don't want to surface that to the caller mid-rollout.
            result = new PaymentResult(false, null, "Payment failed.");
        }

        if (result.Success)
        {
            order.MarkPaid(_clock.UtcNow);
        }
        else
        {
            order.MarkFailed();
        }

        await _repo.UpdateAsync(order, ct).ConfigureAwait(false);
        _logger.OrderProcessed(order.Id, order.Status.ToString());
        return new OrderProcessingResult(result.Success, result.AuthorizationCode, result.FailureReason);
    }
}
