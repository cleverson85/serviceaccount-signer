using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceAccount.Signer.Settings;
using ServiceAccount.Signer.Security;
using ServiceAccount.Signer.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxConcurrentUpgradedConnections = 1000;
    options.Limits.Http2.MaxStreamsPerConnection = 100;
    options.Limits.Http2.InitialConnectionWindowSize = 1 << 20;
    options.Limits.Http2.InitialStreamWindowSize = 1 << 20;

    options.ListenLocalhost(7074, options =>
    {
        options.UseHttps();
        options.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 1 * 1024 * 1024;
    options.MaxSendMessageSize = 512 * 1024;
    options.EnableDetailedErrors = false;
});

builder.Services.AddHealthChecks()
    .AddCheck("signer", () => HealthCheckResult.Healthy());

builder.Services.Configure<SigningOptions>(builder.Configuration.GetSection("Signing"));

builder.Services.AddSingleton<IPrivateKeyProvider, ConfigurePrivateKeyProvider>();
builder.Services.AddSingleton<ITransactionValidator, TransactionValidator>();

var app = builder.Build();

app.MapGrpcService<ServiceAccount.Signer.Services.SignerService>();

app.MapGet("/", () => "Signer Service executing. Use gRPC.");

app.MapHealthChecks("/health"); 

app.Run();

internal partial class Program { }