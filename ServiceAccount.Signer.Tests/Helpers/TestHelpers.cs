using Google.Protobuf;
using Proto;

namespace ServiceAccount.Signer.Tests.Helpers;

public static class TestHelpers
{
    public static TransactionBody MakeValidCryptoTransferBody(long payerNum = 1234)
    {
        var payer = new AccountID { ShardNum = 0, RealmNum = 0, AccountNum = payerNum };

        return new TransactionBody
        {
            TransactionID = new TransactionID { AccountID = payer },
            CryptoTransfer = new CryptoTransferTransactionBody
            {
                Transfers = new TransferList()
            }
        };
    }

    public static byte[] WithUnknownFieldAppended(byte[] bodyBytes)
    {
        uint tag = (uint)((9999 << 3) | 0);
        using var ms = new MemoryStream();
        ms.Write(bodyBytes, 0, bodyBytes.Length);
        var cos = new CodedOutputStream(ms, leaveOpen: true);
        cos.WriteTag(tag);
        cos.WriteInt32(1);
        cos.Flush();
        return ms.ToArray();
    }
}
