# 30 — Cascade delete behavior changed to restrict, breaking data consistency

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_FK_CASCADE_REMOVAL** (cascade delete removal).

## What changed

`src/OrderService/Data/OrderContext.cs` (or OrderService.csproj via Entity Framework configuration) changes
the foreign key delete rule from `CASCADE` to `RESTRICT` for order items.

**Before (safe):**
```csharp
modelBuilder.Entity<OrderItem>()
    .HasOne(oi => oi.Order)
    .WithMany(o => o.Items)
    .HasForeignKey(oi => oi.OrderId)
    .OnDelete(DeleteBehavior.Cascade);
```

**After (regressed):**
```csharp
modelBuilder.Entity<OrderItem>()
    .HasOne(oi => oi.Order)
    .WithMany(o => o.Items)
    .HasForeignKey(oi => oi.OrderId)
    .OnDelete(DeleteBehavior.Restrict);
```

## Why this is risky

- Code that deletes orders now fails with foreign key constraint violations.
- Batch cleanup jobs (archive old orders, retention policies) now throw exceptions.
- The application must be updated to manually delete order items before deleting orders,
  but this change went through without coordinating with the deletion logic.
- Orphaned orders accumulate in the database; disk usage balloons.

## What GauntletCI catches

`GCI_FK_CASCADE_REMOVAL` — `OnDelete(DeleteBehavior.Cascade)` calls changed to `Restrict`,
`SetNull`, or removed. The rule detects when cascade delete rules are relaxed to more
restrictive modes, which breaks existing deletion workflows.

