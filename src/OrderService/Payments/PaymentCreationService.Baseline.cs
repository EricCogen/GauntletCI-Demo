using OrderService.Abstractions;
using OrderService.Payments;

namespace OrderService.Payments;

public class PaymentCreationService
{
    private readonly IPaymentGateway _gateway;
    private readonly IIdempotencyCache _cache;

    public PaymentCreationService(IPaymentGateway gateway, IIdempotencyCache cache)
    {
        _gateway = gateway;
        _cache = cache;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        string idempotencyKey,
        CancellationToken ct)
    {
        var cacheKey = $"payment:idempotency:{idempotencyKey}";
        var cached = await _cache.GetAsync<PaymentResult>(cacheKey, ct);
        if (cached != null)
            return cached;

        var result = await _gateway.ChargeAsync(request, ct);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24), ct);
        return result;
    }
}

public record CreatePaymentRequest(Guid OrderId, decimal Amount, string Currency);
public record PaymentResult(bool Success, string? AuthorizationCode, string? FailureReason);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(CreatePaymentRequest request, CancellationToken ct);
}

public interface IIdempotencyCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct);
}
