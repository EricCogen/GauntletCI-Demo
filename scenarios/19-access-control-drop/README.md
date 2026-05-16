# 19 - Architectural Access Control Drop (Security Regression)

**Expected verdict:** 🔴 CRITICAL - GauntletCI should flag removal of `[Authorize]` attribute.

## What changed
A legacy billing controller endpoint is being refactored to use a clean `MediatR` messaging pipeline. During the conversion, the `[Authorize(Roles = "BillingAdmin")]` attribute is accidentally stripped from the `ProcessRefund` method.

**File:** `src/OrderService/Billing/BillingController.cs`

- **Baseline (main):** Method is decorated with `[Authorize(Roles = "BillingAdmin")]`
- **Inbound PR:** Authorization attribute completely removed during refactoring
- **No compilation error:** The code compiles cleanly; no unit test catches this because the test still mocks the service.

## Why GauntletCI catches this

Traditional tools miss this because:
- **SonarQube:** Snapshot metrics don't detect attribute removal (no syntax violation)
- **StyleCop/Roslyn:** Don't enforce security attribute presence
- **CodeQL:** Doesn't track structural authorization mutations in diffs
- **Unit tests:** Still pass because the refactored method executes the business logic; test setup doesn't verify authorization

**GauntletCI detection:** Compares `IMethodSymbol` models between baseline and PR compilation contexts. The `ProcessRefund` method loses an `AuthorizeAttribute` sub-type with no fallback class-level or handler-level authorization mapping.

**Rule:** Behavioral Change Detection (GCI0003) - Structural mutation of security-critical attributes.

## Risk

A public HTTP endpoint that previously required `BillingAdmin` role is now accessible to any authenticated (or unauthenticated) user. This is a critical security regression that would go undetected in production until exploited.

## Test Coverage

- Integration test validates that unauthorized callers receive 403 Forbidden on baseline
- Same test FAILS on PR code (endpoint returns 200 OK to unauthorized users)
