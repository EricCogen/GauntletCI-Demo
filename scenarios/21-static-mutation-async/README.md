# 21 — Unsynchronized Static State Mutation in Async Flow (Concurrency Regression)

**Expected verdict:** 🔴 CRITICAL — GauntletCI should flag unsynchronized static field mutation inside async method.

## What changed
A telemetry counter is being added to track shipment processing. The developer introduces a static field `_globalShipmentCount` and increments it at the start of the `HandleAsync` method. The code compiles perfectly and single-threaded tests pass. However, under multi-threaded asynchronous load, the counter experiences lost increments due to race conditions.

**File:** `src/OrderService/Telemetry/Handlers/ShipmentHandler.cs`

- **Baseline (main):** No shared mutable state
- **Inbound PR:** Introduces `private static int _globalShipmentCount = 0`
- **Mutation:** Bare `_globalShipmentCount++` without synchronization
- **Context:** Inside an async `Task` method with no synchronization wrapper

## Why GauntletCI catches this

Traditional tools miss this because:
- **SonarQube:** Static fields are common; doesn't flag unsynchronized mutation without explicit linting rules
- **StyleCop:** Allows static fields; mutation inside async methods is not flagged
- **Roslyn Analyzers:** Generic rules don't detect this concurrency anti-pattern
- **Unit tests:** Pass because tests are single-threaded; race condition doesn't manifest
- **Compiler:** Compiles cleanly (no type safety violation)

**GauntletCI detection:** Analyzes `IIncrementExpressionOperation` targeting an `IFieldSymbol` with `IsStatic = true`. Because the operation resides inside a method returning `Task` without a corresponding `Interlocked.Increment` or thread-safe wrapper, a race condition warning is raised.

**Rule:** Behavioral Change Detection (GCI0003) — Unsafe concurrency pattern introduced.

## Risk

Under production load with multiple concurrent requests, the telemetry counter will lose increments. This manifests as silent data corruption in metrics, making observability unreliable and masking operational issues.

## Test Coverage

- Single-threaded unit test PASSES on both baseline and PR (doesn't catch the issue)
- Concurrent integration test with 100 parallel tasks:
  - **Baseline:** Telemetry counter is 0 (no state to corrupt)
  - **PR:** Expected count = 100, actual count < 100 (lost increments due to race condition)
