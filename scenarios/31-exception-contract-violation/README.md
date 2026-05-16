# 31 — Method stops throwing documented exception, breaking exception handling in consumers

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_EXCEPTION_CONTRACT** (exception contract violation).

## What changed

`src/OrderService/Processing/OrderProcessor.cs` previously documented that `ProcessOrder()`
throws `InvalidOperationException` for invalid orders. The regressed version removes the throw,
returning a failure result instead.

**Before (safe):**
```csharp
/// <summary>
/// Process an order. Throws InvalidOperationException if order is invalid.
/// </summary>
public OrderResult ProcessOrder(Order order)
{
    if (order is null) throw new InvalidOperationException("Order cannot be null");
    // Process order
}
```

**After (regressed):**
```csharp
/// <summary>
/// Process an order. Returns failure result if order is invalid.
/// </summary>
public OrderResult ProcessOrder(Order order)
{
    if (order is null) return OrderResult.Failure("Order cannot be null");
    // Process order
}
```

## Why this is risky

- Consumer code expects `InvalidOperationException` and has a try/catch for it.
  Now that exception never fires, and the consumer's error handling is bypassed.
- The calling code may assume an exception means "unrecoverable" but now it just means
  "the result object has an error flag set." These are not the same.
- Logging and monitoring systems that trap exceptions now miss these failures.
- The API contract changed silently without a major version bump.

## What GauntletCI catches

`GCI_EXCEPTION_CONTRACT` — documented exception types (in XML docs or comments) are no longer
thrown by the method. Conversely, new exceptions are thrown that aren't documented.
The rule detects when `throw` statements for specific exception types are removed from methods.

