# 35 — Service locator anti-pattern introduced (dependency resolved from service locator)

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_SERVICE_LOCATOR** (service locator usage).

## What changed

`src/OrderService/Reporting/ReportGenerator.cs` previously accepted a logger via constructor
injection. The regressed version looks up the logger from a global service locator.

**Before (safe):**
```csharp
public class ReportGenerator
{
    private readonly ILogger<ReportGenerator> _logger;

    public ReportGenerator(ILogger<ReportGenerator> logger)
    {
        _logger = logger;
    }

    public Report Generate(Order order)
    {
        _logger.LogInformation("Generating report");
        // Generate report
    }
}
```

**After (regressed):**
```csharp
public class ReportGenerator
{
    public Report Generate(Order order)
    {
        var logger = ServiceLocator.GetService<ILogger<ReportGenerator>>();
        logger.LogInformation("Generating report");
        // Generate report
    }
}
```

## Why this is risky

- Dependencies are now hidden. You can't tell what `ReportGenerator` depends on by reading the
  constructor. You must read the method body.
- Testing becomes harder: you can't inject a mock logger for unit tests.
- The dependency graph is implicit and invisible to the DI container, breaking lifecycle management.
- If the service locator isn't initialized, the method fails at runtime with a null reference,
  not at construction time.
- Service locator is a notorious anti-pattern that hides coupling and makes systems brittle.

## What GauntletCI catches

`GCI_SERVICE_LOCATOR` — calls to `ServiceLocator.GetService()`, `ServiceProvider.GetService()`,
or similar service resolution patterns inside method bodies where constructor injection could
be used instead. The rule detects when the service locator pattern is introduced or expanded.

