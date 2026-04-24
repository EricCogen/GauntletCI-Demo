# 11 — Floating-point equality on a money amount

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0049** (float/double equality comparison).

## What changed
A new `LegacyDiscountCalculator` works in `double` (because it was
ported from a JavaScript pricing prototype) and tests for an exact
zero-net result with `==`:

```csharp
public sealed class LegacyDiscountCalculator
{
    public double NetTotal(double subtotal, double discount)
    {
        var net = subtotal - discount;
        if (net == 0.0)
        {
            return 0.0;
        }
        return net;
    }
}
```

For most inputs `subtotal - discount` is exactly representable, but for
values like `0.1 + 0.2 - 0.3` the result is `5.55e-17`, not `0.0`, and
the comparison silently returns `false`.

## Why this is risky
- Money values must never be compared with `==` in `double`/`float`.
  Binary floating-point cannot exactly represent most decimal fractions,
  so a "zero net" check returns the wrong answer for a single rounding
  ulp difference.
- The bug is path-dependent: it fires only for some discount/subtotal
  pairs, so it sails through unit tests that use round numbers and
  surfaces in production for the one customer with $19.99 and a
  $19.99 discount.
- The fix is either an epsilon comparison (`Math.Abs(net) < 1e-9`) or
  — much better for money — using `decimal` end-to-end.

## What GauntletCI catches
`GCI0049 Float/Double Equality Comparison` — `==` (or `!=`) on the
same line as a floating-point literal in a non-test file.
