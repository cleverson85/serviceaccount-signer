namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class UnknownFieldException : InvalidOperationException
{
    public UnknownFieldException() : base($"{nameof(TransactionValidator)}:Unknown field detected.") { }
}
