# 06 — Concurrency: lock removed from shared counter increment

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0016** (concurrency / non-atomic shared state).

## What changed
`OrderProcessor` gained a `static long _processedCount` for a metrics
rollout, exposed via `ProcessedCount`. Inside `ProcessAsync` the counter
is incremented with the bare `++` operator:

```csharp
private static long _processedCount;
public static long ProcessedCount => _processedCount;

// inside ProcessAsync:
_processedCount++;
```

## Why this is risky
- `OrderProcessor` is registered as **scoped** in DI but `_processedCount`
  is **static** — every concurrent HTTP request races on it.
- `long++` on a 32-bit ABI is **two non-atomic operations**. Reads from
  another thread can observe torn values.
- Updates can be lost: under load the metric will under-count, sometimes
  by a lot. The bug looks like "instrumentation drift" instead of a race.

The fix is one of `Interlocked.Increment(ref _processedCount)`, a
`lock`, or a proper metrics primitive (`Counter<long>`).

## What GauntletCI catches
`GCI0016 Concurrency / non-atomic shared state` — a `static` mutable
field is being mutated without synchronisation in a code path reachable
from concurrent request handling.
