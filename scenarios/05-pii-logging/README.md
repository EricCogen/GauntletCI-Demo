# PII (customer id + raw amount) logged at Info

**Expected verdict:** ⚠️ Warn — `GCI0029 PiiLoggingLeak`

This PR adds a debug log line that includes the customer id and full charge
amount in the success path. Even if it isn't strictly PII in this contrived
example, the rule should warn on logging customer identifiers tied to
financial values.

## What changed
- `OrderProcessor.cs`: added
  `_logger.LogInformation("Charged customer {CustomerId} email={Email} amount={Amount}", ...)`
  with a fabricated email field.

## Why this matters
PII in logs becomes a compliance problem (GDPR, PCI). GauntletCI flags
common leak shapes before they reach prod log aggregators.
