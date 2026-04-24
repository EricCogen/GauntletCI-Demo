# 12 — New public method takes a nullable string with no guard

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0006** (edge case handling).

## What changed
A new `CustomerNoteFormatter` exposes a public method that accepts a
nullable `string?` and immediately dereferences it without any
runtime check:

```csharp
public sealed class CustomerNoteFormatter
{
    public string Format(string? note)
    {
        return note!.Trim().ToUpperInvariant();
    }
}
```

The compiler is silenced with `!` (so the build still succeeds), but
the runtime behaviour is unchanged — calling `Format(null)` throws
`NullReferenceException` deep inside `Trim`.

## Why this is risky
- The signature openly accepts `null` (`string?`), so callers are
  entitled to pass it. Nothing in the body translates that into a
  documented `ArgumentNullException` or a sensible default.
- `NullReferenceException` thrown from a transitive callee is one of
  the worst error shapes to debug: the stack trace points at `Trim`,
  not at the caller that handed in `null`.
- A single `ArgumentNullException.ThrowIfNull(note)` (or a `??`
  fallback) makes the contract explicit.

## What GauntletCI catches
`GCI0006 Edge Case Handling` — a public method whose signature
declares a nullable reference parameter (`string?` / `object?`) and
whose first few lines contain no `null` check, `throw`, or
`ArgumentNullException` guard.
