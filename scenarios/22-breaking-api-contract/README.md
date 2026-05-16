# 22 - Breaking Public Package Contract (No Version Bump)

**Expected verdict:** 🔴 CRITICAL - GauntletCI should flag removal of public API parameter without major version increment.

## What changed
An enterprise notification integration framework interface is being simplified. The developer removes the `CancellationToken` parameter from the public `BroadcastAsync` method. The code compiles cleanly in the local monorepo because all consumers are updated. However, this creates a breaking change for external downstream consumers who have taken a dependency on the NuGet package—their implementations implementing the old interface signature will fail at runtime when called with the new package version.

**File:** `src/OrderService/Contracts/Interfaces/INotificationDispatcher.cs`

- **Baseline (main):** `Task BroadcastAsync(string headline, string payload, CancellationToken ct = default)`
- **Inbound PR:** `Task BroadcastAsync(string headline, string payload)` (parameter removed)
- **Version:** No major version increment in `.csproj` (still 1.x)

## Why GauntletCI catches this

Traditional tools miss this because:
- **SonarQube:** Interface simplification is not a violation
- **CodeQL:** Doesn't track semantic versioning vs API contract changes
- **Compilation:** Internal monorepo compiles because all consumers are updated
- **NuGet publish:** Will publish successfully; breaking change only surfaces when external consumers upgrade

**GauntletCI detection:** Extracts public API contracts via Roslyn symbol comparison. The inbound PR shows a structural contraction of `INotificationDispatcher.BroadcastAsync` parameter list. Package metadata (`.csproj` version) does not indicate a major release increment (e.g., 1.0.0 → 2.0.0), violating semantic versioning rules for public APIs.

**Rule:** Breaking Changes Detection (GCI0012) - Public API structural mutation without version bump.

## Risk

External packages consuming this as a NuGet dependency will break when they upgrade to the new version. Implementation classes that implement the old signature will no longer match the interface definition, causing runtime binding errors. This is silent in CI but catastrophic for consumers in production.

## Test Coverage

- Unit test validates interface contract on baseline (PASS)
- External consumer simulation test:
  - **Baseline:** Calling `BroadcastAsync(headline, payload, cancellationToken)` works
  - **PR:** Same call fails with method signature mismatch (no such overload exists)
