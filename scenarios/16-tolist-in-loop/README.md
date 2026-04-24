# 16 — LINQ scan inside a per-request loop

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0044** (performance hot-path risk).

## What changed
A new `OrderItemDeduplicator` runs a `Where` scan inside a `foreach`
over the same list, materialising it with `.ToList()` on each
iteration:

```csharp
public sealed class OrderItemDeduplicator
{
    public IReadOnlyList<IReadOnlyList<OrderItem>> GroupBySku(Order order)
    {
        var groups = new List<IReadOnlyList<OrderItem>>();
        foreach (var item in order.Items)
        {
            var matches = order.Items
                .Where(i => i.Sku == item.Sku)
                .ToList();
            groups.Add(matches);
        }
        return groups;
    }
}
```

For an order with `n` items this is O(n²). For a single-item order it
is fine. For the larger orders the analytics team has been asking
about it falls over.

## Why this is risky
- The hot path here is the order-processing request, which already
  carries DB and payment latency. Spending O(n²) CPU on a list scan
  is exactly the kind of cost that disappears in dev and surfaces
  during a launch promo.
- The fix is mechanical: hoist the grouping out into a single
  `GroupBy` (or build a `Dictionary<Sku, List<OrderItem>>`) before
  the loop. One pass, O(n).
- "It's only n²" is the most common production-incident footgun
  in this codebase shape; GauntletCI flags it before the n grows.

## What GauntletCI catches
`GCI0044 Performance Hotpath Risk` — a LINQ method (`.Where(`,
`.Select(`, `.FirstOrDefault(`, `.Any(`, `.Count(`) on an added line
nested inside a `for`/`foreach`/`while` loop in a non-test, non-rule
file.
