using Google.Protobuf;
using Grpc.Core;
using NSec.Cryptography;
using ServiceAccount.Signer.Proto;
using ServiceAccount.Signer.Security;
using ServiceAccount.Signer.Validation;

namespace ServiceAccount.Signer.Services;

public sealed class SignerService : TransactionSigner.TransactionSignerBase
{
    private readonly IPrivateKeyProvider _keys;
    private readonly ITransactionValidator _validator;

    public SignerService(IPrivateKeyProvider keys, ITransactionValidator validator)
    {
        _keys = keys;
        _validator = validator;
    }

    public override Task<SignResponse> Sign(SignRequest request, ServerCallContext context)
    {
        try
        {
            ReadOnlySpan<byte> bodySpan = request.TransactionBody.Memory.Span;

            _validator.Validate(bodySpan);

            var key = _keys.GetPrivateKey();
            var signature = SignatureAlgorithm.Ed25519.Sign(key, bodySpan);
            var pubSpan = _keys.PublicKey.Span;

            return Task.FromResult(new SignResponse { SigMap = BuildSignatureMap(signature, pubSpan) });
        }
        catch (InvalidProtocolBufferException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    private SignatureMap BuildSignatureMap(ReadOnlySpan<byte> signature, ReadOnlySpan<byte> pubSpan)
    {
        var signaturePair = new SignaturePair
        {
            PubKeyPrefix = ByteString.CopyFrom(pubSpan),
            Ed25519 = ByteString.CopyFrom(signature)
        };

        var map = new SignatureMap();
        map.SigPair.Add(signaturePair);

        return map;
    }
}

