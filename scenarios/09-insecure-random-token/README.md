# 09 — System.Random used to mint a confirmation token

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI0048** (insecure random in security context).

## What changed
A new helper generates the per-order email confirmation token using
`System.Random`:

```csharp
public sealed class EmailConfirmationTokenGenerator
{
    public string NewConfirmationToken()
    {
        var rng = new Random();
        var token = new char[16];
        for (var i = 0; i < token.Length; i++)
        {
            token[i] = (char)('a' + rng.Next(26));
        }
        return new string(token);
    }
}
```

`System.Random` is a non-cryptographic PRNG. Its output is fully
predictable from its seed, and from observing a handful of values an
attacker can recover the internal state and predict future tokens.

## Why this is risky
- A predictable confirmation token lets an attacker complete the
  confirm-email flow for any pending order without ever receiving the
  email.
- Even ignoring seeding attacks, two `new Random()` instances created
  in the same tick share a seed on older runtimes and emit identical
  sequences — perfect for collision attacks against shared inboxes.
- `RandomNumberGenerator.GetBytes` / `GetHexString` is one line of code
  and is the only correct primitive here.

## What GauntletCI catches
`GCI0048 Insecure Random in Security Context` — `new Random(` appears
within five lines of an identifier that names a security-sensitive
value (here, `token`).
