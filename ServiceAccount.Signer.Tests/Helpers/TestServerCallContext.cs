using Grpc.Core;

namespace ServiceAccount.Signer.Tests.Helpers;

public sealed class TestServerCallContext : ServerCallContext
{
    private readonly string _method;
    private readonly string _host;
    private readonly DateTime _deadline;
    private readonly string _peer;
    private readonly AuthContext _authContext;
    private readonly Metadata _requestHeaders;
    private readonly CancellationToken _cancellationToken;

    private TestServerCallContext(
        string method,
        string host,
        DateTime deadline,
        string peer,
        AuthContext authContext,
        Metadata requestHeaders,
        CancellationToken cancellationToken)
    {
        _method = method;
        _host = host;
        _deadline = deadline;
        _peer = peer;
        _authContext = authContext;
        _requestHeaders = requestHeaders;
        _cancellationToken = cancellationToken;
    }

    public static TestServerCallContext Create(
        string method = "serviceaccount.signer.TransactionSigner/Sign",
        string host = "localhost",
        DateTime? deadline = null,
        string peer = "ipv4:127.0.0.1",
        AuthContext? authContext = null,
        Metadata? requestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        return new TestServerCallContext(
            method,
            host,
            deadline ?? DateTime.UtcNow.AddMinutes(1),
            peer,
            authContext ?? new AuthContext("insecure", new Dictionary<string, List<AuthProperty>>()),
            requestHeaders ?? new Metadata(),
            cancellationToken
        );
    }

    protected override string MethodCore => _method;
    protected override string HostCore => _host;
    protected override string PeerCore => _peer;
    protected override DateTime DeadlineCore => _deadline;
    protected override Metadata RequestHeadersCore => _requestHeaders;
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override Metadata ResponseTrailersCore { get; } = new Metadata();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => _authContext;

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options = null) => 
        throw new NotSupportedException("Context propagation is not supported in TestServerCallContext.");

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        => Task.CompletedTask;
}
