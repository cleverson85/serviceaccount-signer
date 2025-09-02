namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class AllowedPayerAccountInvalidException : InvalidOperationException
{
    public AllowedPayerAccountInvalidException() : base($"{nameof(TransactionValidator)}:Invalid AllowedPayerAccountId. Use the format shard.realm.num (ex.: 0.0.1234).") { }
}
