namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class TransactionBodyEmptyException : InvalidOperationException
{
    public TransactionBodyEmptyException() : base($"{nameof(TransactionValidator)}:Transaction body is empty.") { }
}
