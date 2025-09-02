namespace ServiceAccount.Signer.Security.Exceptions;

public sealed class PrivateKeyNotSetException : InvalidOperationException
{
    public PrivateKeyNotSetException() : base($"{nameof(ConfigurePrivateKeyProvider)}:Ed25519PrivateKeyBase64 not configured.") { }
}
