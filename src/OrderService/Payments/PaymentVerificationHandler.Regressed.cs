using OrderService.Abstractions;
using OrderService.Payments;
using OrderService.Persistence;

namespace OrderService.Payments;

public class PaymentVerificationHandler
{
    private readonly IPaymentClient _payments;
    private readonly IPaymentVerificationService _verification;
    private readonly IOrderRepository _repository;

    public PaymentVerificationHandler(
        IPaymentClient payments,
        IPaymentVerificationService verification,
        IOrderRepository repository)
    {
        _payments = payments;
        _verification = verification;
        _repository = repository;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Order order, CancellationToken ct)
    {
        var result = await _payments.ChargeAsync(
            new PaymentRequest(order.Id, order.Total, order.Customer.Email),
            ct).ConfigureAwait(false);

        if (result.Success)
        {
            _ = _verification.VerifyAsync(result.TransactionId, ct);
        }

        return result;
    }
}

public interface IPaymentVerificationService
{
    Task VerifyAsync(string transactionId, CancellationToken ct);
}
