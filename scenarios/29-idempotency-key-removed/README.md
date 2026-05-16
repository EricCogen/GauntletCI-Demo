# 29 — Duplicate detection (idempotency key) removed from payment creation

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_IDEMPOTENCY_REMOVED** (idempotency key deletion).

## What changed

`src/OrderService/Payments/PaymentCreationService.cs` previously checked for an idempotency key
to prevent duplicate payments. The regressed version removes this check entirely.

**Before (safe):**
```csharp
public async Task<PaymentResult> CreatePaymentAsync(
    CreatePaymentRequest request,
    string idempotencyKey,
    CancellationToken ct)
{
    var cached = await _cache.GetAsync($"idempotency:{idempotencyKey}", ct);
    if (cached != null) return cached;

    var result = await _gateway.ChargeAsync(request, ct);
    await _cache.SetAsync($"idempotency:{idempotencyKey}", result, ct);
    return result;
}
```

**After (regressed):**
```csharp
public async Task<PaymentResult> CreatePaymentAsync(
    CreatePaymentRequest request,
    string idempotencyKey,
    CancellationToken ct)
{
    return await _gateway.ChargeAsync(request, ct);
}
```

## Why this is risky

- Network timeouts, client retries, or double-submit by the user now create duplicate charges.
- Each duplicate charge generates a separate transaction, confusing accounting.
- Customers are charged multiple times for a single order.
- Chargeback rates spike; refund requests skyrocket.
- PCI compliance requires idempotency for payment operations; this removal is a compliance failure.

## What GauntletCI catches

`GCI_IDEMPOTENCY_REMOVED` — parameters, cache lookups, or deduplication logic that were
present before are now removed from payment/financial transaction creation methods.
The rule detects removal of `IdempotencyKey`, `MessageId`, `RequestId`, or similar
dedup-related identifiers.

