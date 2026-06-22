using OrderService.Abstractions;
using OrderService.Payments;

namespace OrderService.Payments;

public class PaymentCreationService
{
    private readonly IPaymentGateway _gateway;

    public PaymentCreationService(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        string idempotencyKey,
        CancellationToken ct)
    {
        return await _gateway.ChargeAsync(request, ct);
    }
}

public record CreatePaymentRequest(Guid OrderId, decimal Amount, string Currency);
public record PaymentResult(bool Success, string? AuthorizationCode, string? FailureReason);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(CreatePaymentRequest request, CancellationToken ct);
}
