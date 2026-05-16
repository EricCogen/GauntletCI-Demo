# 33 — Cache lookup removed during "optimization", causing database load spike

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_CACHE_BYPASS** (cache removal).

## What changed

`src/OrderService/Data/CustomerService.cs` previously checked the cache before hitting the database.
The regressed version removes the cache check to "simplify the code."

**Before (safe):**
```csharp
public async Task<Customer> GetCustomerAsync(Guid customerId, CancellationToken ct)
{
    var cached = await _cache.GetAsync<Customer>($"customer:{customerId}", ct);
    if (cached != null) return cached;

    var customer = await _repository.GetAsync(customerId, ct);
    if (customer != null)
        await _cache.SetAsync($"customer:{customerId}", customer, ct);
    return customer;
}
```

**After (regressed):**
```csharp
public async Task<Customer> GetCustomerAsync(Guid customerId, CancellationToken ct)
{
    return await _repository.GetAsync(customerId, ct);  // Cache bypass
}
```

## Why this is risky

- A hot customer (viewed 1000 times/second) now results in 1000 database queries/second.
- Database connection pool exhaustion leads to request timeouts across the entire application.
- Other database operations (orders, payments) now contend for a starved pool.
- The system degrades under normal load, appearing as a "mysterious" production outage.
- Users see increased latency, timeouts, and error pages.

## What GauntletCI catches

`GCI_CACHE_BYPASS` — cache `.Get()`, `.GetAsync()`, or `TryGetValue()` calls are removed from
hot paths. The rule detects when cache lookups that were protecting database queries
are deleted or commented out.

