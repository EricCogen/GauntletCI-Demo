# 03 — Hardcoded API key in source

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0012** (hardcoded secret).

## What changed
`src/OrderService/Program.cs` adds a `PostConfigure<StripeOptions>` block
that hardcodes the API key into source:

```csharp
builder.Services.PostConfigure<StripeOptions>(opts =>
{
    opts.ApiKey = "gci_demo_7f3a2e9c4b8d6f1a5e2c9b3d4a8e7f6c";
});
```

The literal uses this repo's `gci_demo_{hex}` convention so it doesn't
trigger any provider-specific scanner — but **GauntletCI's `GCI0012`
heuristic recognises the credential-shaped string assigned to a
property named `ApiKey`** and flags it.

## Why this is risky
- Anything checked into source is permanently leaked, even if you remove
  it later (git history is forever).
- Static analyzers like GitHub secret scanning would block real Stripe
  keys (`sk_live_…`, `pk_live_…`) at push time, but a maintainer can
  bypass that. GauntletCI catches it first.

## What GauntletCI catches
`GCI0012 Hardcoded secret` — credential-shaped literal assigned to an
options property named `ApiKey`.
