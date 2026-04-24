# 04 — Breaking change to public PaymentClient signature

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0004** (breaking public API change).

## What changed
`IPaymentClient.ChargeAsync` lost its `CancellationToken ct = default`
parameter — purportedly because nobody was passing one through. Five files
were updated to compile against the new signature:

- `src/OrderService/Payments/IPaymentClient.cs` — interface definition
- `src/OrderService/Payments/PaymentClient.cs` — implementation
- `src/OrderService/Processing/OrderProcessor.cs` — call site
- `src/OrderService/Controllers/PaymentsController.cs` — call site
- `tests/OrderService.Tests/Fakes/FakePaymentClient.cs` — test fake

## Why this is risky
- `IPaymentClient` is a `public interface` — any downstream package
  implementing it (a custom adapter, a mock) **will fail to compile** after
  upgrading.
- Removing a parameter is a binary-incompatible change even if no internal
  caller uses it.
- The diff looks like a routine cleanup, but it's a SemVer-major change.

## What GauntletCI catches
`GCI0004 Breaking public API change` — a public/protected member's
signature changed in a non-additive way (parameter removed).
