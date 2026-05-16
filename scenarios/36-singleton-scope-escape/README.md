# 36 — Singleton captures scoped dependency, violating DI scope constraints

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_SCOPE_VIOLATION** (singleton captures scoped).

## What changed

`src/OrderService/Logging/LoggingService.cs` is registered as a singleton but is injected with
a scoped `UserContext` dependency. The scoped dependency is now held by the singleton, living
longer than it should.

**Before (safe):**
```csharp
services.AddScoped<UserContext>();
services.AddScoped<LoggingService>();  // Scoped matches its dependencies

public class LoggingService
{
    private readonly UserContext _userContext;
    public LoggingService(UserContext userContext) => _userContext = userContext;
}
```

**After (regressed):**
```csharp
services.AddScoped<UserContext>();
services.AddSingleton<LoggingService>();  // Singleton + scoped = scope violation!

public class LoggingService
{
    private readonly UserContext _userContext;
    public LoggingService(UserContext userContext) => _userContext = userContext;
}
```

## Why this is risky

- The `UserContext` instance from the first request is captured by the singleton `LoggingService`.
- Every subsequent request now sees the first user's context, not their own.
- User A's request logs with User B's ID. User B's operations are attributed to User A.
- Multi-tenancy breaks: requests from different tenants bleed into each other.
- This is a critical security and data isolation violation.

## What GauntletCI catches

`GCI_SCOPE_VIOLATION` — a singleton or transient service is injected with a scoped dependency,
or vice versa (a scoped service injected with a transient). The rule detects scope mismatches
in the dependency injection configuration.

