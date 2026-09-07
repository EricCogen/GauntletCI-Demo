using System.Security.Cryptography;
using System.Text;
using OrderService.Abstractions;

namespace OrderService.Security;

public class EncryptionService
{
    private readonly IEncryptionKeyProvider _keyProvider;

    public EncryptionService(IEncryptionKeyProvider keyProvider)
    {
        _keyProvider = keyProvider;
    }

    public async Task<string> DecryptAsync(string ciphertext, CancellationToken ct)
    {
        var key = await _keyProvider.GetCurrentKeyAsync(ct);
        return Decrypt(ciphertext, key.Material);
    }

    private string Decrypt(string ciphertext, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;

        var buffer = Convert.FromBase64String(ciphertext);
        aes.IV = buffer.Take(aes.IV.Length).ToArray();
        var encrypted = buffer.Skip(aes.IV.Length).ToArray();

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encrypted);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);
        return sr.ReadToEnd();
    }
}

public class CryptoException : Exception
{
    public CryptoException(string message) : base(message) { }
}

public interface IEncryptionKeyProvider
{
    Task<EncryptionKey> GetCurrentKeyAsync(CancellationToken ct);
}

public record EncryptionKey(byte[] Material, DateTime CreatedAt);
