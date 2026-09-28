using OrderService.Payments;

namespace OrderService.Tests.Fakes;

public sealed class FakePaymentClient : IPaymentClient
{
    public Func<PaymentRequest, PaymentResult> Responder { get; set; }
        = _ => new PaymentResult(true, "AUTH-FAKE", null);

    public List<PaymentRequest> Calls { get; } = new();

    public Task<PaymentResult> ChargeAsync(PaymentRequest request)
    {
        Calls.Add(request);
        return Task.FromResult(Responder(request));
    }
}
