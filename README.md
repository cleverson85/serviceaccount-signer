
# Transaction Signing Service

A gRPC service written in **.NET 9** that receives Hedera transactions (`TransactionBody`), validates business rules, and returns Ed25519 signatures in the `SignatureMap` format.

---

## ✨ Overview

This project implements a **transaction signing service** that complies with the technical specification:

- **gRPC Service**: exposes the `Sign` method defined in `signer.proto`.
- **Input**: serialized bytes of a `TransactionBody` (HAPI).
- **Output**: `SignatureMap` containing `SignaturePair`(s).
- **Security**:
  - Ed25519 private key loaded **once** at startup with [NSec.Cryptography](https://www.nuget.org/packages/NSec.Cryptography).
  - Private key material is **never logged, serialized, or exposed**.
  - Raw byte array is zeroed in memory after import.
  - `Key` is disposed on shutdown.
- **Validation**:
  - **Payer** must match the configured value in `SigningOptions`.
  - Only **HBAR CryptoTransfer** transactions are accepted.
  - **TokenTransfers** and **NftTransfers** are rejected.
  - Transactions with `UnknownFields` are rejected.

---

## 📂 Project Structure

```
ServiceAccount.sln
│
├── ServiceAccount.Signer/         # gRPC Service
│   ├── Protos/                    # signer.proto (service contract)
│   ├── third_party/hapi/          # HAPI .proto files (Hedera messages)
│   ├── Services/                  # SignerService (gRPC implementation)
│   ├── Validation/                # TransactionValidator and rules
│   └── Settings/                  # SigningOptions (allowed payer config)
│
├── ServiceAccount.Signer.Tests/   # Unit tests (xUnit)
│   ├── Unit/                      # Validator and service tests
│   └── Helpers/                   # Test utilities

```

---

## ⚙️ Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- gRPC tooling (`Grpc.Net.Client`, `Grpc.AspNetCore`, `Google.Protobuf`, `Grpc.Tools`)
- [NSec.Cryptography](https://www.nuget.org/packages/NSec.Cryptography)

---

## ▶️ Running the Service

From the `ServiceAccount.Signer` folder:

```bash
dotnet run
```

The gRPC server listens by default on:

- `https://localhost:7074` (TLS dev)

Configure **allowed payer** and **private key** in `appsettings.json`:

```json
{
  "SigningOptions": {
    "AllowedPayerAccountId": "0.0.1234",
    "Ed25519PrivateKeyBase64": "<your-private-key-in-base64>"
  }
}
```

---

## 🧪 Running Tests

Unit tests cover all success and failure scenarios of the validator and service:

```bash
dotnet test
```

Tests validated:

- `AllowedPayerAccountNotConfiguredException`
- `AllowedPayerAccountInvalidException`
- `TransactionBodyEmptyException`
- `TransactionAccountMissingException`
- `PayerAccountNotAllowedException`
- `CryptoTransferNotAllowedException`
- `TransferTokenNotAllowedException`
- `NftTransferNotAllowedException`
- `UnknownFieldException`
- Valid scenario (signature successfully generated)

---

## 📜 License

Project developed for technical assessment purposes.  
Restricted use according to test context.
