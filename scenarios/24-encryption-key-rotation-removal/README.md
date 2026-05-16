# 24 — Encryption key rotation logic removed, breaking decryption of old data

**Expected verdict:** ❌ Fails — GauntletCI should fire **GCI_CRYPTO_KEY_ROTATION** (key rotation removal).

## What changed

`src/OrderService/Security/EncryptionService.cs` had multi-key rotation support for
transitioning between encryption keys. The logic now only supports a single current key,
silently failing to decrypt data encrypted with old keys.

**Before (safe):**
```csharp
private async Task<string> DecryptAsync(string ciphertext, CancellationToken ct)
{
    var keys = await _keyProvider.GetAllKeysAsync(ct);
    foreach (var key in keys.OrderByDescending(k => k.CreatedAt))
    {
        try { return Decrypt(ciphertext, key); }
        catch { /* Try next key */ }
    }
    throw new CryptoException("Could not decrypt with any known key");
}
```

**After (regressed):**
```csharp
private async Task<string> DecryptAsync(string ciphertext, CancellationToken ct)
{
    var key = await _keyProvider.GetCurrentKeyAsync(ct);
    return Decrypt(ciphertext, key);  // Fails silently if old key was used
}
```

## Why this is risky

- Customers with historical encrypted data (SSN, payment info) lose access to it.
- Silent failures: no exception, no log entry. The system returns null or empty string.
- Key rotation is a compliance and security requirement (PCI-DSS, HIPAA). Removing it can
  trigger audit failures and breach notifications.
- Backup recovery becomes impossible: old encrypted backups cannot be decrypted.

## What GauntletCI catches

`GCI_CRYPTO_KEY_ROTATION` — encryption/decryption logic paths that previously tried multiple
keys now only try one. The rule detects removal of `foreach` or `for` loops iterating over key lists,
or removal of `try/catch` chains attempting decryption with fallback keys.

