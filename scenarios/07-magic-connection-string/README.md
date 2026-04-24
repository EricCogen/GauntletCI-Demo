# 07 — Hardcoded SQL connection string in Program.cs

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0010** (hardcoded connection string).

## What changed
`src/OrderService/Program.cs` registers an order-database connection string
as a constant directly in code, instead of binding it from `IConfiguration`:

```csharp
// Order-database connection — temporarily inlined while the
// secret-store wiring is being moved out of the legacy host.
const string ordersDbConnection =
    "Server=tcp:orders-db.internal;Database=Orders;Integrated Security=true;TrustServerCertificate=true";
builder.Services.AddSingleton(new OrdersDbConnectionString(ordersDbConnection));
```

The literal contains the canonical `Server=` connection-string marker,
which GauntletCI's `GCI0010` rule recognises as an environment-coupled
configuration value baked into source.

## Why this is risky
- A connection string in source ties the binary to one environment —
  promoting the same artefact to staging or production now requires a
  rebuild.
- Even when the literal carries no password (as here), the host name
  leaks internal infrastructure topology into every artifact and CI log.
- It removes the ability to rotate the endpoint without a code change,
  which is exactly what configuration is for.

## What GauntletCI catches
`GCI0010 Hardcoding and Configuration` — string literal containing a
`Server=` connection-string marker added to a non-test file.
