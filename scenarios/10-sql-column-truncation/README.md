# 10 — EF column too short for real-world input

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0050** (SQL column truncation risk).

## What changed
A new EF entity configuration under `Persistence/Migrations/` declares
the customer-email column as `[StringLength(50)]`:

```csharp
public sealed class OrderCustomerSchema
{
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [StringLength(80)]
    public string DisplayName { get; set; } = string.Empty;
}
```

50 characters is well below the 320-character limit that RFC 5321
permits for an email address, and 80 is below most "full name" inputs
once accents, scripts, and suffixes are included. SQL Server (and
EF Core's default mapping) will silently **truncate** anything longer
without raising an error.

## Why this is risky
- Truncation is silent — the application returns success while the row
  it stored is wrong. Users discover this when their forgot-password
  email never arrives.
- Truncating an email at 50 characters can change *who* the row points
  to (`alice.long-surname@corp.example.com` becomes
  `alice.long-surname@corp.exa`), which is a security incident, not a
  display issue.
- Adding server-side validation that rejects long inputs is a one-line
  change; widening the column is a one-line migration. Either is correct.
  Doing neither is the bug.

## What GauntletCI catches
`GCI0050 SQL Column Truncation Risk` — `[StringLength(N)]` (or
`HasMaxLength(N)` / `nvarchar(N)`) with `N < 100` in a file under a
path containing `migration` / `dbcontext` / `entityconfig` / `schema`.
