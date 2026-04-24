# 08 — HttpClient field never disposed

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0024** (resource lifecycle).

## What changed
A new `StripeWebhookClient` class holds a `HttpClient` as a private
field that is constructed inline and never disposed:

```csharp
public sealed class StripeWebhookClient
{
    private readonly HttpClient _http = new HttpClient();

    public Task<HttpResponseMessage> AcknowledgeAsync(Uri callback, CancellationToken ct)
        => _http.PostAsync(callback, content: null, ct);
}
```

The class itself does not implement `IDisposable`, so the underlying
socket handle outlives every request that uses it.

## Why this is risky
- `HttpClient` implements `IDisposable`. Owning one as a field without
  also implementing `IDisposable` (or routing through `IHttpClientFactory`)
  leaks the socket pool slot for the lifetime of the process.
- Without a `using`/`Dispose()` pair anywhere in scope, exceptions thrown
  before the next request also leak any pending response stream.
- The instance is shared across all callers of `StripeWebhookClient`, so
  a single misbehaving callback can starve the whole socket pool.

## What GauntletCI catches
`GCI0024 Resource Lifecycle` — `new HttpClient(` allocated as a field
with no `using`, no surrounding `Dispose()` call, and no nearby
`IHttpClientFactory` reference.

> **Known co-fire:** GauntletCI's `GCI0039 External Service Safety` rule
> also flags the bare `new HttpClient(` instantiation (and the missing
> explicit `Timeout`). Both rules describe the same root cause from
> different angles; the lifecycle finding is the headline issue this
> scenario is built to demonstrate.
