using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using System;

namespace Chromia.Postchain.Ft3
{
    public class RateLimit
    {
        public int Points;
        [JsonProperty(PropertyName = "last_update")]
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

        public static IEnumerator ExecFreeOperation(string accountID, Blockchain blockchain, Action onSuccess)
        {
            var op = AccountDevOperations.FreeOp(accountID);

            var request = blockchain.Connection.NewTransaction(new byte[][] { }, (string error) => { UnityEngine.Debug.Log(error); });
            request.AddOperation(op.Name, op.Args);
            yield return request.PostAndWait(onSuccess);
        }

        public static IEnumerator GetByAccountRateLimit(string id, Blockchain blockchain, Action<RateLimit> onSuccess)
        {
            yield return blockchain.Query<RateLimit>("ft3.get_account_rate_limit_last_update",
                new List<(string, object)>() { ("account_id", id) }.ToArray(),
                onSuccess,
                (string error) => { }
            );
        }

        public static IEnumerator GivePoints(string accountID, int points, Blockchain blockchain, Action onSuccess)
        {
            var op = AccountDevOperations.GivePoints(accountID, points);

            var request = blockchain.Connection.NewTransaction(new byte[][] { }, (string error) => { UnityEngine.Debug.Log(error); });
            request.AddOperation(op.Name, op.Args);
            yield return request.PostAndWait(onSuccess);
        }

        public static IEnumerator GetLastTimestamp(Blockchain blockchain, Action<long> onSuccess)
        {
            yield return blockchain.Query<long>("ft3.get_last_timestamp",
                new List<(string, object)>().ToArray(),
                onSuccess,
                (string error) => { }
            );
        }

        public static IEnumerator GetPointsAvailable(int points, int lastOperation, Blockchain blockchain, Action<int> onSuccess)
        {
            var maxCount = blockchain.Info.RateLimitInfo.MaxPoints;
            var recoveryTime = blockchain.Info.RateLimitInfo.RecoveryTime;
            var lastTimestamp = 0L;

            yield return GetLastTimestamp(blockchain, (long _lastTimeStamp) => { lastTimestamp = _lastTimeStamp; });
            decimal delta = lastTimestamp - lastOperation;

            var pointsAvailable = (int)Math.Floor(delta / recoveryTime) + points;
            if (pointsAvailable > maxCount)
            {
                onSuccess(maxCount);
            }

            if (pointsAvailable > 0)
            {
                onSuccess(pointsAvailable);
            }
            else
            {
                onSuccess(0);
            }
        }
    }
}