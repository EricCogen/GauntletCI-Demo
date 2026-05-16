# 23 — Role-based authorization check moved inside conditional

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_AUTH_BYPASS** (authorization check repositioning).

## What changed

`src/OrderService/Controllers/AdminPolicyController.cs` has the `[Authorize(Policy="AdminOnly")]`
attribute moved from the method level into a conditional branch inside the method body.

**Before (safe):**
```csharp
[Authorize(Policy="AdminOnly")]
public async Task<ActionResult> UpdateSystemSettings(SystemSettingsRequest request, CancellationToken ct)
{
    // Update settings logic
}
```

**After (regressed):**
```csharp
public async Task<ActionResult> UpdateSystemSettings(SystemSettingsRequest request, CancellationToken ct)
{
    if (User.HasClaim("role", "admin"))
    {
        // Update settings logic
    }
    return Ok();
}
```

## Why this is risky

- The attribute-based authorization enforces access *before* method entry. Moving it inside
  creates a window where an unauthorized user reaches the method and can inspect its
  signature, parameters, and error messages.
- If the conditional check is incomplete or misses an edge case (null User, expired claims),
  the endpoint becomes accessible to non-admins.
- Distributed caching layers, proxies, and test frameworks may not respect inline checks,
  leading to permission escapes in production.
- The regressed code still returns `Ok()` even when the user lacks permission, which is
  confusing (should be 403, not 200).

## What GauntletCI catches

`GCI_AUTH_BYPASS` — authorization decorators (`[Authorize]`, `[Authorize(Policy=...)]`) removed
or replaced with inline conditional checks. The rule detects when public method signatures
lose their declarative authorization markers in favor of imperative ones.

