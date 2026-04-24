# 01 — Safe typo / wording fix (clean)

**Expected verdict:** ✅ Clean — GauntletCI should produce no findings.

## What changed
A cosmetic fix in `src/OrderService/Logging/RequestLogger.cs`:

- Capitalized `"HTTP"` to `"Http"` in the structured log format string.
- Renamed the `cid=` field label to `correlationId=` for readability.

No behavioural change. No new dependencies. No public-surface change.
This is the kind of PR you want your CI to wave through.

## Why this scenario exists
GauntletCI must be quiet on safe changes. If this PR comes back with any
finding, the noise/precision balance is off.
