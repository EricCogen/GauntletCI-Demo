namespace OrderService.Payments;

public interface IPaymentClient
{
    Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct = default);
}
