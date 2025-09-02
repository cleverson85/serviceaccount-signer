using Proto;

namespace ServiceAccount.Signer.Validation.Exceptions;

public sealed class CryptoTransferNotAllowedException : InvalidOperationException
{
    public CryptoTransferNotAllowedException(TransactionBody body) : base($"{nameof(TransactionValidator)}:Only CryptoTransfer is allowed (received: {body.DataCase}).") { }
}
