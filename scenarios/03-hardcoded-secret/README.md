# Hardcoded API key in source

**Expected verdict:** 🛑 Block — `GCI0012 SecurityRisk` (and likely
`GCI0010 HardcodingAndConfiguration`)

This PR replaces the env-var-driven `PAYMENT_API_KEY` with a literal token
checked into source. GauntletCI should block the commit.

## What changed
- `Program.cs`: removed `Environment.GetEnvironmentVariable("PAYMENT_API_KEY")`
  fallback and inlined a hardcoded API-key-shaped literal. (The literal
  here is intentionally non-matching of any real provider format so GitHub
  secret scanning doesn't intercept the push — but it is exactly the shape
  GCI0012 looks for.)

## Why this matters
Secrets in source are the single most-cited cause of credential leaks. Any
pre-commit tool worth running must catch this pattern.
