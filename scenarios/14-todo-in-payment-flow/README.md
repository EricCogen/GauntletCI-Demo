# 14 — TODO comment on the payment success path

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0042** (TODO/stub detection).

## What changed
`PaymentClient.ChargeAsync` gains a `// TODO` marker in the
success-result branch, deferring downstream webhook emission:

```csharp
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
```

The change is one comment line, but it sits on the live payment
success path — exactly the kind of silent stub that ships and never
gets revisited.

## Why this is risky
- A `TODO` on a money path is a pending guarantee to the rest of the
  system. Reconciliation, fraud, ledger, and accounting jobs all
  expect that webhook to fire.
- TODOs in production code rot: they outlive the engineer who wrote
  them and the Slack thread that explained them.
- The "right" outcome of this finding is either to do the work now,
  or to file an explicit issue and link it from the comment so the
  intent is tracked outside the source.

## What GauntletCI catches
`GCI0042 TODO/Stub Detection` — added line in a non-test file
contains `TODO` (also `FIXME`, `HACK`, or `throw new
NotImplementedException`) and is not an XML doc-comment line.
