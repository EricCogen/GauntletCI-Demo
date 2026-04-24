# 15 — Retry endpoint POSTed without an idempotency key

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0022** (idempotency & retry safety).

## What changed
`PaymentsController` gains a new `Retry` action exposed as
`POST /payments/{id}/retry`, with no idempotency-key parameter, header,
or dedup check anywhere in the handler:

```csharp
[HttpPost("{id:guid}/retry")]
public async Task<ActionResult<PaymentResult>> Retry(
    Guid id,
    [FromBody] ChargeRequest request,
    CancellationToken ct)
{
    var result = await _payments.ChargeAsync(
        new PaymentRequest(request.OrderId, new Money(request.Amount, request.Currency), request.CustomerEmail),
        ct);
    return result.Success ? Ok(result) : UnprocessableEntity(result);
}
```

A retry endpoint is the single most likely thing to be invoked twice —
and "twice" on a charge endpoint means a duplicate charge.

## Why this is risky
- Network retries, queue redelivery, and angry users hammering the
  retry button all funnel here. Without an idempotency key, every one
  of them attempts a fresh charge.
- The Stripe API takes an `Idempotency-Key` header for exactly this
  reason; not propagating one through this layer disables it.
- The fix is two lines: accept an `Idempotency-Key` header, hash the
  request, and short-circuit on a cache hit.

## What GauntletCI catches
`GCI0022 Idempotency & Retry Safety` — a `[HttpPost(...)]` endpoint
added to the diff with no `IdempotencyKey` / `Idempotency-Key` /
`RequestId` / `MessageId` / `dedup` signal anywhere in the surrounding
window.
