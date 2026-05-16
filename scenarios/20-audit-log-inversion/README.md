# 20 - Failure-Path Audit Log Inversion (CFG Mutation)

**Expected verdict:** 🔴 HIGH - GauntletCI should flag control flow reordering where audit logging bypasses exception paths.

## What changed
An order fulfillment coordinator method is being performance-optimized. The developer moves the audit logging invocation from the start of the method to the end, to minimize response delays. However, this creates a critical behavioral regression: if the core fulfillment engine throws an unhandled exception, the audit log is never recorded—breaking compliance requirements.

**File:** `src/OrderService/Fulfillment/Services/FulfillmentCoordinator.cs`

- **Baseline (main):** Audit logging happens BEFORE `ProcessAsync` is called
- **Inbound PR:** Audit logging moved AFTER `ProcessAsync` is called
- **Silent failure:** The code compiles cleanly; exceptions still bubble up; but the audit trail is lost

## Why GauntletCI catches this

Traditional tools miss this because:
- **SonarQube:** Reordering statements is not a code smell (no violation flagged)
- **CodeQL:** Doesn't track execution sequence mutations or compliance-critical code ordering
- **Unit tests:** Still pass if they don't simulate exceptions in `ProcessAsync`
- **Linters:** See no syntax violations

**GauntletCI detection:** Runs `ControlFlowAnalysis` on both method bodies. Baseline shows `LogActionAsync -> ProcessAsync` edge flow. PR shows `ProcessAsync -> LogActionAsync` edge flow. This is flagged as an un-synchronized sequence regression that violates control-flow integrity.

**Rule:** Behavioral Change Detection (GCI0003) - Critical execution sequence reordering.

## Risk

Audit compliance violations. In regulated industries (finance, healthcare), missing audit trails can trigger regulatory fines and audit failures. If `ProcessAsync` throws an exception, the fulfillment attempt is never logged.

## Test Coverage

- Integration test validates audit log entry is created on success (PASS on baseline)
- Integration test validates audit log entry is created EVEN IF an exception occurs (FAIL on PR code — log is never written)
