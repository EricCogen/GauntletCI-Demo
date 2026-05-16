# 26 — Lock scope narrowed, exposing previously protected code to race conditions

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_LOCK_SCOPE_REDUCTION** (narrowed synchronization).

## What changed

`src/OrderService/Accounting/TransactionCounter.cs` previously protected both the counter
increment AND the ledger write inside a single `lock` block. The regressed version moves
the ledger write outside, creating a race condition between increment and write.

**Before (safe):**
```csharp
public void RecordTransaction(Transaction tx)
{
    lock (_mutex)
    {
        _counter++;
        _ledger.Add(tx);  // Protected
    }
}
```

**After (regressed):**
```csharp
public void RecordTransaction(Transaction tx)
{
    lock (_mutex)
    {
        _counter++;
    }
    _ledger.Add(tx);  // Now unprotected!
}
```

## Why this is risky

- Two threads can race: one increments the counter, another writes to the ledger, then another
  thread sees a counter that is one higher than the ledger entries.
- Audits show mismatched transaction counts and ledger entries.
- Financial reconciliation breaks: "We processed 1001 transactions but only have 1000 ledger entries."
- The inconsistency is non-deterministic, making it near-impossible to reproduce in testing.

## What GauntletCI catches

`GCI_LOCK_SCOPE_REDUCTION` — code that was previously inside a `lock(...)` block is moved
outside. The rule detects when the `lock` scope shrinks by comparing the surface area of
critical sections before and after the change.

