# 34 — Connection pooling disabled, changing to per-request connections

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_POOL_CONFIG_MUTATION** (pooling disabled).

## What changed

`src/OrderService/Data/DbContext.cs` configuration removes the connection string pooling setting,
or reduces the pool size dramatically, or disables it entirely.

**Before (safe):**
```csharp
var connectionString = "Data Source=localhost;Initial Catalog=OrderDb;" +
    "Integrated Security=true;Pooling=true;Max Pool Size=100;";
var options = new DbContextOptionsBuilder<OrderContext>()
    .UseSqlServer(connectionString)
    .Build();
```

**After (regressed):**
```csharp
var connectionString = "Data Source=localhost;Initial Catalog=OrderDb;" +
    "Integrated Security=true;Pooling=false;";  // Pooling disabled!
var options = new DbContextOptionsBuilder<OrderContext>()
    .UseSqlServer(connectionString)
    .Build();
```

## Why this is risky

- Each database operation now creates a fresh connection, negotiating TCP handshake, TLS,
  and authentication with the database server.
- Connection creation overhead becomes the bottleneck. Latency per query increases 10-100x.
- The database sees a connection storm; it exhausts file handles and memory.
- The application becomes "slow" without any code changes being obvious.
- Recovery requires a restart of both the application and database server.

## What GauntletCI catches

`GCI_POOL_CONFIG_MUTATION` — connection string settings for `Pooling=false`, `Max Pool Size=1`,
or removal of pooling configuration. The rule detects changes to database connection configuration
that disable or severely restrict pooling.

