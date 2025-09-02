using Grpc.Core;

namespace ServiceAccount.Signer.Services.Exceptions;

public sealed class TransactionBodyEmptyException : RpcException
{
    public TransactionBodyEmptyException() : base(new Status(StatusCode.InvalidArgument, $"{nameof(SignerService):transaction_body empty.}")) { }
}
