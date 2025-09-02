using Microsoft.Extensions.Options;
using NSec.Cryptography;
using ServiceAccount.Signer.Settings;
using ServiceAccount.Signer.Security.Exceptions;
using System.Security.Cryptography;

namespace ServiceAccount.Signer.Security;

public sealed class ConfigurePrivateKeyProvider : IPrivateKeyProvider
{
    private readonly Key _key;
    private readonly ReadOnlyMemory<byte> _pub;
    private bool _disposed;

    public ReadOnlyMemory<byte> PublicKey => _pub;

    public ConfigurePrivateKeyProvider(IOptions<SigningOptions> options)
    {
        var base64PrivateKey = options.Value.Ed25519PrivateKeyBase64 ?? throw new PrivateKeyNotSetException();
        var raw = Convert.FromBase64String(base64PrivateKey);

        if (raw.Length != 32) 
            throw new InvalidBytesLengthException(raw.Length);

        _key = Key.Import(SignatureAlgorithm.Ed25519, raw, KeyBlobFormat.RawPrivateKey);

        CryptographicOperations.ZeroMemory(raw);

        _pub = _key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
    }

    public Key GetPrivateKey() => _key;
    public void Dispose()
    {
        if (_disposed) 
            return;

        _key.Dispose();               // NSec zera/descarta buffer nativo da chave
        _disposed = true;
    }
}

public interface IPrivateKeyProvider : IDisposable
{
    ReadOnlyMemory<byte> PublicKey { get; }
    Key GetPrivateKey(); 
}
