# 02 — Silent exception swallow in payment path

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0007** (silent catch).

## What changed
`src/OrderService/Processing/OrderProcessor.cs` was updated to make the
payment call "more resilient" by wrapping it in a try/catch.

The catch block swallows **every** exception (`catch { ... }`) and turns it
into a generic `PaymentResult(false, null, "Payment failed.")` — no logging,
no rethrow, no telemetry. Transient errors, programmer errors, cancellation,
all become indistinguishable failures.

## Why this is risky
- Production incidents become invisible: you'll never know *why* charges fail.
- `OperationCanceledException` and `OutOfMemoryException` get swallowed too.
- The catch hides bugs that would otherwise surface in development.

## What GauntletCI catches
`GCI0007 Silent exception swallow` — the catch block has no logging, no
re-throw, and produces a result that cannot be distinguished from a
business-level decline.
