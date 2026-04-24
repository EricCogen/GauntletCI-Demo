# Concurrency: lock removed from shared counter increment

**Expected verdict:** 🛑 Block — `GCI0016 ConcurrencyAndStateRisk`

This PR drops the `lock (_sync) { _processedCount++; }` and replaces it
with a bare `_processedCount++`. Under concurrent `ProcessAsync` calls the
counter will lose updates. GauntletCI should detect the unprotected mutation
of shared state.

## What changed
- `OrderProcessor.cs`: `lock (_sync) { _processedCount++; }` → `_processedCount++;`
  (the field is still read inside a lock from `ProcessedCount`, so the
  read/write protocol is now inconsistent — also a classic smell).

## Why this matters
Race conditions are notoriously hard to reproduce and rarely caught by
unit tests. Catching them statically at PR time avoids a class of "works
on my machine" production bugs.
