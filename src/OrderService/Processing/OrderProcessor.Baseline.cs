using OrderService.Abstractions;

namespace OrderService.Processing;

public class OrderProcessor
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentClient _payments;

    public OrderProcessor(IOrderRepository repository, IPaymentClient payments)
    {
        _repository = repository;
        _payments = payments;
    }

    /// <summary>
    /// Process an order for payment and fulfillment.
    /// Throws InvalidOperationException if the order is invalid.
    /// </summary>
    public async Task<OrderResult> ProcessOrderAsync(Order order, CancellationToken ct)
    {
        if (order is null)
            throw new InvalidOperationException("Order cannot be null");

        if (order.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item");

        var result = await _payments.ChargeAsync(
            new PaymentRequest(order.Id, order.Total, order.Customer.Email), ct);

        return new OrderResult(result.Success, result.AuthorizationCode, result.FailureReason);
    }
}

public record OrderResult(bool Success, string? TransactionId, string? FailureReason);
