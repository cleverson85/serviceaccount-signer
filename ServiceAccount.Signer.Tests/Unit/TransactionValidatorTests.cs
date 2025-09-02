using Google.Protobuf;
using Microsoft.Extensions.Options;
using ServiceAccount.Signer.Settings;
using ServiceAccount.Signer.Validation;
using ServiceAccount.Signer.Validation.Exceptions;
using System.Reflection;
using Hapi = Proto;

namespace ServiceAccount.Signer.Tests.Unit;

public class TransactionValidatorTests
{
    private static IOptions<SigningOptions> Opt(string payer = "0.0.1234")
        => Options.Create(new SigningOptions { AllowedPayerAccountId = payer });

    private static byte[] Bytes(Hapi.TransactionBody body) => body.ToByteArray();

    [Fact]
    public void Ctor_WhenAllowedPayerMissing_Throws()
    {
        var opt = Options.Create(new SigningOptions { AllowedPayerAccountId = null });
        Assert.Throws<AllowedPayerAccountNotConfiguredException>(() => new TransactionValidator(opt));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0.0")]
    [InlineData("a.b.c")]
    [InlineData("0.0.xyz")]
    public void Ctor_WhenAllowedPayerInvalid_Throws(string bad)
    {
        var opt = Options.Create(new SigningOptions { AllowedPayerAccountId = bad });
        Assert.Throws<AllowedPayerAccountInvalidException>(() => new TransactionValidator(opt));
    }

    [Fact]
    public void Validate_WhenBodyEmpty_Throws()
    {
        var v = new TransactionValidator(Opt());
        Assert.Throws<TransactionBodyEmptyException>(() => v.Validate(ReadOnlySpan<byte>.Empty));
    }

    [Fact]
    public void Validate_WhenTransactionIdMissing_Throws()
    {
        var v = new TransactionValidator(Opt());

        var body = new Hapi.TransactionBody
        {
            CryptoTransfer = new Hapi.CryptoTransferTransactionBody { Transfers = new Hapi.TransferList() }
        };

        Assert.Throws<TransactionAccountMissingException>(() => v.Validate(Bytes(body)));
    }

    [Fact]
    public void Validate_WhenPayerNotAllowed_Throws()
    {
        var v = new TransactionValidator(Opt("0.0.1"));
        var body = Helpers.TestHelpers.MakeValidCryptoTransferBody(1234);

        Assert.Throws<PayerAccountNotAllowedException>(() => v.Validate(Bytes(body)));
    }

    [Fact]
    public void Validate_WhenNotCryptoTransfer_Throws()
    {
        var v = new TransactionValidator(Opt());

        var payer = new Hapi.AccountID { ShardNum = 0, RealmNum = 0, AccountNum = 1234 };
        var body = new Hapi.TransactionBody
        {
            TransactionID = new Hapi.TransactionID { AccountID = payer },
            CryptoCreateAccount = new Hapi.CryptoCreateTransactionBody() // outro tipo -> proibido
        };

        Assert.Throws<CryptoTransferNotAllowedException>(() => v.Validate(Bytes(body)));
    }

    [Fact]
    public void Validate_WhenTokenTransfersPresent_Throws()
    {
        var v = new TransactionValidator(Opt());
        var payer = new Hapi.AccountID { ShardNum = 0, RealmNum = 0, AccountNum = 1234 };

        var body = new Hapi.TransactionBody
        {
            TransactionID = new Hapi.TransactionID { AccountID = payer },
            CryptoTransfer = new Hapi.CryptoTransferTransactionBody
            {
                Transfers = new Hapi.TransferList(),
                TokenTransfers = { new Hapi.TokenTransferList() }
            }
        };

        Assert.Throws<TransferTokenNotAllowedException>(() => v.Validate(Bytes(body)));
    }

    [Fact]
    public void Validate_WhenNftTransfersPresent_Throws()
    {
        var v = new TransactionValidator(Opt());
        var body = Helpers.TestHelpers.MakeValidCryptoTransferBody(1234);

        var nftProp = body.CryptoTransfer!
                          .GetType()
                          .GetProperty("NftTransfers", BindingFlags.Public | BindingFlags.Instance);

        if (nftProp is null)
            return;

        var nftList = nftProp!.GetValue(body.CryptoTransfer);
        var addMethod = nftList!.GetType().GetMethod("Add");
        var elemType = addMethod!.GetParameters()[0].ParameterType;
        var nftItem = Activator.CreateInstance(elemType)!;
        addMethod.Invoke(nftList, new[] { nftItem });

        Assert.Throws<NftTransferNotAllowedException>(() => v.Validate(Bytes(body)));
    }

    [Fact]
    public void Validate_WhenUnknownFieldsPresent_Throws()
    {
        var v = new TransactionValidator(Opt());
        var body = Helpers.TestHelpers.MakeValidCryptoTransferBody(1234);
        var bytes = Helpers.TestHelpers.WithUnknownFieldAppended(body.ToByteArray());

        Assert.Throws<UnknownFieldException>(() => v.Validate(bytes));
    }

    [Fact]
    public void Validate_WhenOk_Passes()
    {
        var v = new TransactionValidator(Opt());
        var body = Helpers.TestHelpers.MakeValidCryptoTransferBody(1234);

        var ex = Record.Exception(() => v.Validate(Bytes(body)));
        Assert.Null(ex);
    }
}
