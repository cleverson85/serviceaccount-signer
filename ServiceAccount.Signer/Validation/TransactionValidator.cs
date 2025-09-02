using Google.Protobuf;
using Microsoft.Extensions.Options;
using Proto;
using ServiceAccount.Signer.Settings;
using ServiceAccount.Signer.Validation.Exceptions;

namespace ServiceAccount.Signer.Validation;

public sealed class TransactionValidator : ITransactionValidator
{
    private readonly long _allowedShard, _allowedRealm, _allowedNum;

    public TransactionValidator(IOptions<SigningOptions> options)
    {
        var id = (options.Value.AllowedPayerAccountId ?? throw new AllowedPayerAccountNotConfiguredException())
                        .Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (id.Length != 3
            || !long.TryParse(id[0], out _allowedShard)
            || !long.TryParse(id[1], out _allowedRealm)
            || !long.TryParse(id[2], out _allowedNum))
        {
            throw new AllowedPayerAccountInvalidException();
        }
    }

    public void Validate(ReadOnlySpan<byte> bodyBytes)
    {
        if (bodyBytes.IsEmpty)
            throw new TransactionBodyEmptyException();

        var body = TransactionBodyParser(bodyBytes);

        PayerValidate(body);
        CryptoTransferOnlyValidate(body);
        NoTokensValidate(body.CryptoTransfer!);
        NoNftValidate(body.CryptoTransfer!);
        NoUnknownFieldValidate(body);

    }

    private static TransactionBody TransactionBodyParser(ReadOnlySpan<byte> bodyBytes) => TransactionBody.Parser.ParseFrom(bodyBytes);

    private void PayerValidate(TransactionBody transactionBody)
    {
        var payer = transactionBody.TransactionID?.AccountID ?? throw new TransactionAccountMissingException();

        if (IsPayerAllowed(payer))
            throw new PayerAccountNotAllowedException(payer);
    }

    private bool IsPayerAllowed(AccountID payer) => payer.ShardNum != _allowedShard || payer.RealmNum != _allowedRealm || payer.AccountNum != _allowedNum;

    private static void CryptoTransferOnlyValidate(TransactionBody transactionBody)
    {
        if (transactionBody.DataCase != TransactionBody.DataOneofCase.CryptoTransfer || transactionBody.CryptoTransfer is null)
            throw new CryptoTransferNotAllowedException(transactionBody);
    }

    private static void NoTokensValidate(CryptoTransferTransactionBody cryptoTransferTransactionBody)
    {
        if (cryptoTransferTransactionBody.TokenTransfers != null && cryptoTransferTransactionBody.TokenTransfers.Count > 0)
            throw new TransferTokenNotAllowedException();
    }

    private static void NoNftValidate(CryptoTransferTransactionBody cryptoTransferTransactionBody)
    {
        var nftProp = cryptoTransferTransactionBody.GetType().GetProperty("NftTransfers");
        var nftValue = nftProp?.GetValue(cryptoTransferTransactionBody) as System.Collections.ICollection;

        if (nftValue is { Count: > 0 })
            throw new NftTransferNotAllowedException();
    }

    private static void NoUnknownFieldValidate(IMessage root)
    {
        UnknownFieldsAccessorValidator.EnsureNoUnknownFields(root);
    }
}

public interface ITransactionValidator { void Validate(ReadOnlySpan<byte> bodyBytes); }