# Breaking change to public PaymentClient signature

**Expected verdict:** 🛑 Block — `GCI0004 BreakingChangeRisk`

This PR removes `apiKey` from `PaymentClient`'s constructor (now read from a
static field). Any external caller constructing `PaymentClient` will fail to
compile. GauntletCI should detect the public-surface break.

## What changed
- `PaymentClient.cs`: constructor signature
  `(HttpClient, ILogger, string apiBaseUrl, string apiKey)` →
  `(HttpClient, ILogger, string apiBaseUrl)`.
- `Program.cs`: in-repo call site updated to compile.
- **External callers** of `PaymentClient` (other repos, downstream
  consumers) will still break — and *that* is exactly what GCI0004 flags
  by inspecting the public surface diff itself, regardless of whether
  internal callers got fixed.

## Why this matters
Signature-level breaking changes are silent at the diff level until you
trace every caller. GCI0004 surfaces them at PR review time.
