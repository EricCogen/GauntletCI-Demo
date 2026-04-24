# 13 — New `throw new Exception(...)` path with no test coverage

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0032** (uncaught exception path).

## What changed
`PricingService` gains a new validation helper that throws a bare
`System.Exception` when an amount is negative:

```csharp
public Money RequireNonNegative(Money amount)
{
    if (amount.Amount < 0m)
    {
        throw new Exception($"Amount must be non-negative: {amount.Amount}");
    }
    return amount;
}
```

The diff adds a `throw new` in production code without adding any
corresponding `Assert.Throws<>` / `Should().Throw<>()` test in the
test project.

## Why this is risky
- `throw new Exception(...)` is the wrong base type. Callers cannot
  `catch` something more specific without also catching every other
  bug. The .NET guidelines explicitly call this out.
- A new exception path with no test is, by definition, a path no one
  has ever executed. The first time it fires will be in production.
- The trivial fix is to (a) throw `ArgumentOutOfRangeException`
  (which has dedicated handling everywhere in .NET) and (b) add an
  `Assert.Throws<ArgumentOutOfRangeException>(...)` test.

## What GauntletCI catches
`GCI0032 Uncaught Exception Path` — one or more `throw new` statements
in non-test files in the diff, with no corresponding throw-assertion
(`Assert.Throws`, `Should().Throw`, `ThrowsAsync`, `Throws<…>`) in any
test file in the same diff.
