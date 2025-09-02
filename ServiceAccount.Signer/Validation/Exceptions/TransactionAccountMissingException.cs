namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class TransactionAccountMissingException : InvalidOperationException
{
    public TransactionAccountMissingException() : base($"{nameof(TransactionValidator)}:TransactionId/AccountId is missing.") { }
}
