using Proto;

namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class PayerAccountNotAllowedException : InvalidOperationException
{
    public PayerAccountNotAllowedException(AccountID payer) : base($"{nameof(TransactionValidator)}:Payer account is not allowed - {payer.ShardNum}.{payer.RealmNum}.{payer.AccountNum}.") { }
}
