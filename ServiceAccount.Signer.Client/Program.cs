using Google.Protobuf;
using Grpc.Net.Client;
using Proto;                       // HAPI: TransactionBody, etc.
using ServiceAccount.Signer.Proto; // signer.proto (cliente gRPC)
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // ===== 1) construir um TransactionBody válido (HBAR only) =====
        var payerId = new AccountID { ShardNum = 0, RealmNum = 0, AccountNum = 1234 }; // == AllowedPayerAccountId
        var txId = new TransactionID { AccountID = payerId };

        var body = new TransactionBody
        {
            TransactionID = txId,
            // CryptoTransfer com TransferList vazia (teste mínimo; pode adicionar AccountAmount se quiser)
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = new TransferList() }
        };
        byte[] bodyBytes = body.ToByteArray();

        //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        //// ===== 2) criar canal gRPC para HTTP/2 sem TLS (h2c) na porta 8080 =====
        //var handler = new SocketsHttpHandler();
        //using var httpClient = new HttpClient(handler);

        var channel = GrpcChannel.ForAddress("https://localhost:7074");

        // ===== 3) invocar o RPC =====
        var client = new TransactionSigner.TransactionSignerClient(channel);
        var reply = await client.SignAsync(new SignRequest { TransactionBody = ByteString.CopyFrom(bodyBytes) });

        // ===== 4) imprimir o resultado =====
        Console.WriteLine($"Assinaturas: {reply.SigMap.SigPair.Count}");
        foreach (var p in reply.SigMap.SigPair)
        {
            Console.WriteLine($"- pubKeyPrefix bytes: {p.PubKeyPrefix.Length}, ed25519 sig bytes: {p.Ed25519.Length}");
        }
    }
}
