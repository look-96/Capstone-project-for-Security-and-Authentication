using System.Security.Cryptography;
using System.Text;

namespace SecureDataApp.Models;

/// <summary>
/// Demonstrates encrypted in-memory storage. It is not a substitute for a real
/// identity provider, authorization system, or managed key store.
/// </summary>
public sealed class SecureStorage : IDisposable
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;
    private readonly object _sync = new();
    private EncryptedPayload? _payload;
    private bool _disposed;

    public SecureStorage(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException("AES keys must be 16, 24, or 32 bytes long.", nameof(key));
        }

        _key = key.ToArray();
    }

    public void StoreData(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        ThrowIfDisposed();

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        CryptographicOperations.ZeroMemory(plaintextBytes);

        lock (_sync)
        {
            ThrowIfDisposed();
            if (_payload is not null)
            {
                _payload.Clear();
            }

            _payload = new EncryptedPayload(nonce, ciphertext, tag);
        }
    }

    public string RetrieveData(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access denied. Admin role required.");
        }

        lock (_sync)
        {
            ThrowIfDisposed();
            var payload = _payload ?? throw new InvalidOperationException("No data has been stored.");
            var plaintext = new byte[payload.Ciphertext.Length];

            using (var aes = new AesGcm(_key, TagSize))
            {
                aes.Decrypt(payload.Nonce, payload.Ciphertext, payload.Tag, plaintext);
            }

            try
            {
                return Encoding.UTF8.GetString(plaintext);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }
    }

    /// <summary>Returns ciphertext for the console demonstration; do not expose this in a user-facing API.</summary>
    public string GetEncryptedDataForDiagnostics()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            var payload = _payload ?? throw new InvalidOperationException("No data has been stored.");
            var combined = new byte[payload.Nonce.Length + payload.Ciphertext.Length + payload.Tag.Length];
            Buffer.BlockCopy(payload.Nonce, 0, combined, 0, payload.Nonce.Length);
            Buffer.BlockCopy(payload.Ciphertext, 0, combined, payload.Nonce.Length, payload.Ciphertext.Length);
            Buffer.BlockCopy(payload.Tag, 0, combined, payload.Nonce.Length + payload.Ciphertext.Length, payload.Tag.Length);
            return Convert.ToBase64String(combined);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            CryptographicOperations.ZeroMemory(_key);
            _payload?.Clear();
            _payload = null;
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed record EncryptedPayload(byte[] Nonce, byte[] Ciphertext, byte[] Tag)
    {
        public void Clear()
        {
            CryptographicOperations.ZeroMemory(Nonce);
            CryptographicOperations.ZeroMemory(Ciphertext);
            CryptographicOperations.ZeroMemory(Tag);
        }
    }
}
