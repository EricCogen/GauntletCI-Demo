# 05 — PII logged in payment success path

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0029** (PII in logs).

## What changed
A new `LogInformation` call in `OrderProcessor.ProcessAsync` writes the
customer's **email address** and the **raw charge amount** to logs after a
successful payment, ostensibly "for the analytics rollout":

```csharp
_logger.LogInformation(
    "Charged customer {Email} {Amount} {Currency} (order {OrderId})",
    order.Customer.Email, priced.Total.Amount, priced.Total.Currency, order.Id);
```

## Why this is risky
- Email addresses are personal data under GDPR/CCPA. Logs are routinely
  shipped to third-party aggregators (Datadog, Splunk, CloudWatch) that
  may not be in your data-processor agreement.
- "Just for the rollout" log lines outlive their reason; they end up in
  six months of cold storage that nobody audits.
- The amount + email pair is identifying enough to correlate users across
  systems.

## What GauntletCI catches
`GCI0029 PII in logs` — a structured log argument named `Email` (or
matching email-like patterns) is being emitted to a logger.
