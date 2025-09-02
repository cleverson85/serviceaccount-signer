namespace ServiceAccount.Signer.Validation.Exceptions;

public class NftTransferNotAllowedException : InvalidCastException
{
    public NftTransferNotAllowedException() : base($"{nameof(TransactionValidator)}:NftTransfers is not allowed.") { }
}
