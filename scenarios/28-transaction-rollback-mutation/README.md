# 28 — Rollback point moved in exception handler, committing partial changes

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_TRANSACTION_SCOPE_MUTATION** (rollback repositioning).

## What changed

`src/OrderService/Data/OrderTransactionManager.cs` previously called `transaction.Rollback()` at
the end of an exception handler. The regressed version moves the rollback *before* a partial
commit, allowing some changes to persist.

**Before (safe):**
```csharp
try
{
    tx.BeginTransaction();
    _repo.UpdateInventory(order);
    _repo.CreateShipment(order);
    tx.Commit();
}
catch (Exception ex)
{
    tx.Rollback();  // Rollback everything
    _logger.Error($"Order failed: {ex}");
}
```

**After (regressed):**
```csharp
try
{
    tx.BeginTransaction();
    _repo.UpdateInventory(order);
    _repo.CreateShipment(order);
    tx.Commit();
}
catch (Exception ex)
{
    _logger.Error($"Order failed: {ex}");
    tx.Rollback();  // Rollback after commit? Or moved before?
}
```

## Why this is risky

- Partial updates: inventory is decremented but shipment is never created.
- Customers can check out but never receive orders.
- Stock reconciliation becomes impossible.
- A single transient error (network timeout, deadlock) leaves the database in an inconsistent state.

## What GauntletCI catches

`GCI_TRANSACTION_SCOPE_MUTATION` — `Rollback()` calls moved or removed from exception handlers,
or `Commit()` moved earlier in the control flow. The rule detects when transaction boundaries
(Begin, Commit, Rollback) change their relative positions or conditions.

