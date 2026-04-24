using Microsoft.Extensions.Logging;
using OrderService.Models;

namespace OrderService;

public sealed class OrderProcessor
{
    private readonly PaymentClient _payments;
    private readonly ILogger<OrderProcessor> _logger;
    private readonly object _sync = new();
    private int _processedCount;

    public OrderProcessor(PaymentClient payments, ILogger<OrderProcessor> logger)
    {
        _payments = payments ?? throw new ArgumentNullException(nameof(payments));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int ProcessedCount
    {
        get { lock (_sync) { return _processedCount; } }
    }

    public async Task<ProcessResult> ProcessAsync(Order order, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.Amount <= 0)
        {
            return ProcessResult.Rejected("Amount must be positive");
        }

        try
        {
            var charged = await _payments.ChargeAsync(order, ct).ConfigureAwait(false);
            if (!charged)
            {
                return ProcessResult.Failed("Payment declined");
            }

            lock (_sync) { _processedCount++; }
            var email = $"{order.CustomerId}@example.com";
            _logger.LogInformation(
                "Charged customer {CustomerId} email={Email} amount={Amount} {Currency}",
                order.CustomerId, email, order.Amount, order.Currency);
            return ProcessResult.Ok();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Payment gateway unreachable for order {OrderId}", order.Id);
            return ProcessResult.Failed("Payment gateway unreachable");
        }
    }
}

public sealed record ProcessResult(bool Success, string? Reason)
{
    public static ProcessResult Ok() => new(true, null);
    public static ProcessResult Rejected(string reason) => new(false, reason);
    public static ProcessResult Failed(string reason) => new(false, reason);
}
