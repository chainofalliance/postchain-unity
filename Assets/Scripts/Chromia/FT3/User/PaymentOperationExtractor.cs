using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Ft3
{
    public class PaymentOperationExtractor
    {
        private readonly byte[] _transaction;
        private readonly string _chainId;

        public PaymentOperationExtractor(byte[] rawTransaction, string chainId)
        {
            this._transaction = rawTransaction;
            this._chainId = chainId;
        }

        // public PaymentOperation[] Extract()
        // {
        //     List<PaymentOperation> paymentOperations = new List<PaymentOperation>();
        //     GTXValue value = PostchainUtil.DeserializeGTX(this._transaction);
        //     dynamic[] transaction = value.Array[0].ToObjectArray();

        //     foreach (var operation in transaction[1])
        //     {
        //         if(((string) operation[0]).Equals("ft3.transfer"))
        //         {
        //             paymentOperations.Add(
        //                 PaymentOperation.FromTransfer(TransferOperation.From(operation), this._chainId)
        //             );
        //         }
        //         else if(((string) operation[0]).Equals("ft3.xc.init_xfer"))
        //         {
        //             paymentOperations.Add(
        //                 PaymentOperation.FromXTransfer(XTransferOperation.From(operation), this._chainId)
        //             );
        //         }
        //     }           

        //     return paymentOperations.ToArray();
        // }
    }
}