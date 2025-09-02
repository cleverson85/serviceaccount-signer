namespace ServiceAccount.Signer.Settings;

public sealed class SigningOptions
{
    public string? AllowedPayerAccountId { get; set; }
    public string? Ed25519PrivateKeyBase64 { get; set; }
}