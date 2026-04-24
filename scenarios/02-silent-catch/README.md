# Silent exception swallow in payment path

**Expected verdict:** 🛑 Block — `GCI0007 ErrorHandlingIntegrity`

This PR replaces a logged catch with `catch { }`, hiding HTTP failures from
operators. GauntletCI should flag the silent swallow on the critical payment
path.

## What changed
- `OrderProcessor.cs`: `catch (HttpRequestException ex) { logger.LogError(...); return Failed(...); }` →
  `catch { return Ok(); }`

## Why this matters
This is one of the most common production-incident patterns: an exception
that gets eaten so the caller thinks the operation succeeded. Static rules
like GCI0007 catch this before merge.
