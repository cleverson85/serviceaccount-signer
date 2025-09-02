namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class TransferTokenNotAllowedException : InvalidOperationException
{
    public TransferTokenNotAllowedException() : base($"{nameof(TransactionValidator)}:TokenTransfers is not allowed.") { }
}
