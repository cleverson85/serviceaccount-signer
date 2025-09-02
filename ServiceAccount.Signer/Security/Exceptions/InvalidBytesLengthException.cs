namespace ServiceAccount.Signer.Security.Exceptions;

public sealed class InvalidBytesLengthException : InvalidOperationException
{
    public InvalidBytesLengthException(int actualLength) : base($"{nameof(ConfigurePrivateKeyProvider)}:Expected 32 raw bytes, got {actualLength}.") { }
}
