# 25 — Async method called without await (fire-and-forget breaks sequencing)

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_FIRE_AND_FORGET** (unawaited async call).

## What changed

`src/OrderService/Payments/PaymentVerificationHandler.cs` calls `VerifyAsync()` but
removes the `await` keyword. The verification now runs in the background after the response
is already sent to the client.

**Before (safe):**
```csharp
public async Task<PaymentResult> ProcessPaymentAsync(Order order, CancellationToken ct)
{
    var result = await _payments.ChargeAsync(order.Total, ct);
    if (result.Success)
    {
        await _verification.VerifyAsync(result.TransactionId, ct);
    }
    return result;
}
```

**After (regressed):**
```csharp
public async Task<PaymentResult> ProcessPaymentAsync(Order order, CancellationToken ct)
{
    var result = await _payments.ChargeAsync(order.Total, ct);
    if (result.Success)
    {
        _verification.VerifyAsync(result.TransactionId, ct);  // No await!
    }
    return result;
}
```

## Why this is risky

- The verification task runs after the response is sent. If it throws, no one is listening.
- If the application shuts down before the task completes, verification never happens.
- The client receives success before the full workflow is validated, creating a race condition.
- Logs and metrics for this task will fire asynchronously, making debugging nearly impossible.

## What GauntletCI catches

`GCI_FIRE_AND_FORGET` — assignment of a `Task` or `Task<T>` from an async method call
without an `await` or `.Wait()`. The rule detects task expressions that are discarded
or assigned to ignored variables, indicating fire-and-forget semantics.

