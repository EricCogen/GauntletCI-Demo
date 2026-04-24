# 17 — Singleton background service captures a scoped dependency

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0038** (Dependency Injection Safety — captive dependency).

## What changed
A new `OrderReminderBackgroundService` is registered as a **singleton**
hosted service alongside a brand-new `IOrderEventEmitter` registered as
**scoped**, all inside the same DI composition root:

```csharp
// New background service that nudges customers about pending orders.
builder.Services.AddSingleton<OrderReminderBackgroundService>();
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<OrderReminderBackgroundService>());

// New scoped emitter so per-request correlation can flow into events.
builder.Services.AddScoped<IOrderEventEmitter, OrderEventEmitter>();
```

The new background service constructor takes `IOrderEventEmitter`
directly:

```csharp
public OrderReminderBackgroundService(IOrderEventEmitter emitter)
{
    _emitter = emitter;
}
```

## Why this is risky
- A singleton resolves its dependencies **once**, at the moment the
  container builds it. A scoped service injected directly into a
  singleton becomes a **captive dependency** — the singleton holds the
  same scoped instance forever, across every request and every scope.
- For `IOrderEventEmitter`, this means a per-request correlation id is
  frozen on first use and reused for every subsequent event — silently
  attributing every emitted event to the original request.
- The bug is invisible in unit tests (which build a fresh container per
  test) and only manifests under real traffic, often weeks after deploy
  when a debugging engineer notices that "every error has the same
  trace id."

## What GauntletCI catches
`GCI0038 Dependency Injection Safety` — the diff registers both an
`AddSingleton<…>` and an `AddScoped<…>` lifetime in the same file
(`Program.cs`), the canonical shape of a captive-dependency wiring
mistake.

## How to fix it
- Inject `IServiceScopeFactory` into the singleton instead, and create
  a scope per work item to resolve `IOrderEventEmitter` fresh each time.
- Or promote the emitter to singleton if it is genuinely stateless.
- Or convert the background service itself into a hosted-scope pattern
  (`IServiceScopeFactory.CreateScope()` inside `ExecuteAsync`).
