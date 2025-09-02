using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Options;
using NSec.Cryptography;
using ServiceAccount.Signer.Security;
using ServiceAccount.Signer.Services;
using ServiceAccount.Signer.Settings;
using ServiceAccount.Signer.Tests.Helpers;
using ServiceAccount.Signer.Validation;

namespace ServiceAccount.Signer.Tests.Unit;

public class SignerServiceUnitTests
{
    private static IPrivateKeyProvider MakeKeys()
    {
        var raw = new byte[32];
        var b64 = Convert.ToBase64String(raw);

        var opt = Options.Create(new SigningOptions
        {
            AllowedPayerAccountId = "0.0.1234",
            Ed25519PrivateKeyBase64 = b64
        });

        return new ConfigurePrivateKeyProvider(opt);
    }

    [Fact]
    public async Task Sign_Returns_SigMap_And_Verifies()
    {
        using var keys = MakeKeys();
        var validator = new TransactionValidator(Options.Create(new SigningOptions { AllowedPayerAccountId = "0.0.1234" }));
        var svc = new SignerService(keys, validator);

        var body = TestHelpers.MakeValidCryptoTransferBody(1234);
        var bytes = body.ToByteArray();

        var reply = await svc.Sign(new Proto.SignRequest { TransactionBody = ByteString.CopyFrom(bytes) }, TestServerCallContext.Create());

        Assert.Single(reply.SigMap.SigPair);
        var p = reply.SigMap.SigPair[0];
        Assert.Equal(32, p.PubKeyPrefix.Length);
        Assert.Equal(64, p.Ed25519.Length);

        var algo = SignatureAlgorithm.Ed25519;
        var pub = PublicKey.Import(algo, p.PubKeyPrefix.ToByteArray(), KeyBlobFormat.RawPublicKey);

        Assert.True(algo.Verify(pub, bytes, p.Ed25519.ToByteArray()));
    }

    [Fact]
    public async Task InvalidBytes_Maps_To_InvalidArgument()
    {
        using var keys = MakeKeys();
        var validator = new TransactionValidator(Options.Create(new SigningOptions { AllowedPayerAccountId = "0.0.1234" }));
        var svc = new SignerService(keys, validator);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            svc.Sign(new Proto.SignRequest { TransactionBody = ByteString.CopyFrom(new byte[] { 1, 2, 3 }) }, TestServerCallContext.Create())
        );

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task BusinessRule_Breaks_Maps_To_FailedPrecondition()
    {
        using var keys = MakeKeys();
        var validator = new TransactionValidator(Options.Create(new SigningOptions { AllowedPayerAccountId = "0.0.9999" }));
        var svc = new SignerService(keys, validator);

        var body = TestHelpers.MakeValidCryptoTransferBody(1234);
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            svc.Sign(new Proto.SignRequest { TransactionBody = ByteString.CopyFrom(body.ToByteArray()) }, TestServerCallContext.Create())
        );

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }
}
