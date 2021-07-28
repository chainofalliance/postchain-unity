using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Chromia.Postchain.Ft3
{
    public class RateLimit
    {
        public int Points;
        public int LastUpgrade;

        public RateLimit(int points, int lastUpgrade)
        {
            Points = points;
            LastUpgrade = lastUpgrade;
        }

        public int GetRequestsLeft()
        {
            return Points;
        }

        // public static async Task ExecFreeOperation(byte[] accountID, Blockchain blockchain)
        // {
        //     var txBuilder = blockchain.CreateTransactionBuilder();
        //     txBuilder.AddOperation(AccountDevOperations.FreeOp(accountID));
        //     txBuilder.AddOperation(AccountOperations.Nop());
        //     var tx = txBuilder.Build(new List<byte[]>());
        //     await tx.Post();
        // }

        // public static async Task<RateLimit> GetByAccountRateLimit(byte[] accountID, Blockchain blockchain)
        // {
        //     var rateInfo = await blockchain.Connection.Gtx.Query<RateLimit>("ft3.get_account_rate_limit", ("account_id", Util.ByteArrayToString(accountID)));

        //     if (rateInfo.control.Error)
        //     {
        //         return null;
        //     }

        //     return rateInfo.content;
        // }

        // public static async Task GivePoints(byte[] accountID, int points, Blockchain blockchain)
        // {
        //     var txBuilder = blockchain.CreateTransactionBuilder();
        //     txBuilder.AddOperation(AccountDevOperations.GivePoints(accountID, points));
        //     txBuilder.AddOperation(AccountOperations.Nop());
        //     var tx = txBuilder.Build(new List<byte[]>());
        //     await tx.Post();
        // }

        // public static async Task<long> GetLastTimestamp(Blockchain blockchain)
        // {
        //     var ts = await blockchain.Connection.Gtx.Query<long>("ft3.get_last_timestamp");
        //     return ts.content;
        // }

        // public static async Task<int> GetPointsAvailable(int points, int lastOperation, Blockchain blockchain)
        // {
        //     var maxCount = blockchain.Info.RequestMaxCount;
        //     var recoveryTime = blockchain.Info.RequestRecoveryTime;
        //     var lastTimestamp = await GetLastTimestamp(blockchain);
        //     decimal delta = lastTimestamp - lastOperation;

        //     var pointsAvailable = (int)Math.Floor(delta / recoveryTime) + points;
        //     if (pointsAvailable > maxCount)
        //     {
        //         return maxCount;
        //     }
        //     return pointsAvailable > 0 ? pointsAvailable : 0;
        // }
    }
}