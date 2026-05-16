# 32 — Numeric conversion logic simplified, changing behavior for edge cases

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_TYPE_COERCION_CHANGE** (conversion logic removal).

## What changed

`src/OrderService/Pricing/PricingCalculator.cs` changes from an explicit decimal conversion
with boundary checking to an implicit cast that silently truncates.

**Before (safe):**
```csharp
private decimal ConvertCurrency(double sourceAmount, decimal exchangeRate)
{
    var converted = (decimal)sourceAmount * exchangeRate;
    if (converted > MaxTransactionAmount)
        throw new OverflowException("Amount exceeds maximum");
    return converted;
}
```

**After (regressed):**
```csharp
private decimal ConvertCurrency(double sourceAmount, decimal exchangeRate)
{
    return (decimal)sourceAmount * exchangeRate;  // No bounds check
}
```

## Why this is risky

- Edge cases (very large or very small numbers) may now silently overflow or truncate.
- Prices calculated with the old logic differ from prices calculated with the new logic.
- For a specific customer with a specific exchange rate, the price suddenly changes.
- Precision loss in financial calculations leads to discrepancies in accounts payable.
- The change is subtle: the code "looks" the same, but the boundary check is missing.

## What GauntletCI catches

`GCI_TYPE_COERCION_CHANGE` — explicit type conversions (casts, `.ToDecimal()`, `.Parse()`)
are replaced with implicit ones. Boundary checks (`>`, `<`, `MaxValue`, `MinValue`) are removed
from numeric conversion paths. The rule detects when type safety is relaxed.

