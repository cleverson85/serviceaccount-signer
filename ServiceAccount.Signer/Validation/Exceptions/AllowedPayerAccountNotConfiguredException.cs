namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class AllowedPayerAccountNotConfiguredException : InvalidOperationException
{
    public AllowedPayerAccountNotConfiguredException() : base($"{nameof(TransactionValidator)}:AllowedPayerAccountId not configured.") { }
}
