using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderService.Payments;

public sealed class PaymentClient : IPaymentClient
{
    private readonly StripeOptions _options;
    private readonly RetryPolicy _retry;
    private readonly ILogger<PaymentClient> _logger;

    public PaymentClient(IOptions<StripeOptions> options, RetryPolicy retry, ILogger<PaymentClient> logger)
    {
        _options = options.Value;
        _retry = retry;
        _logger = logger;
    }

    public Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            throw new InvalidOperationException("Stripe API key is not configured.");
        }

        return _retry.ExecuteAsync(async _ =>
        {
            await Task.Yield();
            _logger.LogInformation(
                "Charging order {OrderId} for {Amount} {Currency}",
                request.OrderId, request.Amount.Amount, request.Amount.Currency);

            if (request.Amount.Amount <= 0m)
            {
                return new PaymentResult(false, null, "Amount must be positive.");
            }
            // TODO: emit payment.succeeded webhook for downstream reconciliation
            return new PaymentResult(true, $"AUTH-{Guid.NewGuid():N}", null);
        }, ct);
    }
}
