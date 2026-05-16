# 27 — Blocking on async task in request context (sync-over-async anti-pattern)

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_SYNC_OVER_ASYNC** (task.result blocking).

## What changed

`src/OrderService/Execution/AsyncOperationExecutor.cs` changes from `await` to `.Result`
to synchronously block on an async task. In ASP.NET request contexts, this causes deadlocks.

**Before (safe):**
```csharp
public OrderResult ExecuteOrder(Order order)
{
    var result = await _processor.ProcessAsync(order);
    return result;
}
```

**After (regressed):**
```csharp
public OrderResult ExecuteOrder(Order order)
{
    var result = _processor.ProcessAsync(order).Result;  // Blocks!
    return result;
}
```

## Why this is risky

- In ASP.NET, blocking a thread pool thread is terrible for scalability. The thread cannot
  service other requests until the task completes.
- If the async task tries to marshal back to the request context (e.g., `ConfigureAwait(true)`),
  a deadlock occurs: the blocking thread is holding the context, and the async task needs it.
- Under load, the thread pool becomes exhausted waiting for tasks that will never unblock.
- Symptoms: "mysterious" request timeouts that only happen under high concurrency.

## What GauntletCI catches

`GCI_SYNC_OVER_ASYNC` — `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` called on a
`Task` or `Task<T>` from an async method in a `public` or `async` context. The rule detects
sync-blocking patterns in ASP.NET controllers and high-level async methods.

