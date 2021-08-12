
using System.Collections.Generic;
using System.Collections;
using System;

namespace Chromia.Postchain.Ft3
{
    public class AssetBalance
    {
        public long Amount;
        public Asset Asset;

        public AssetBalance(long amount, Asset asset)
        {
            this.Amount = amount;
            this.Asset = asset;
        }

        public static IEnumerator GetByAccountId(string id, Blockchain blockchain, Action<AssetBalance[]> onSuccess)
        {
            yield return blockchain.Query<AssetBalanceQuery[]>("ft3.get_asset_balances", new List<(string, object)>() { ("account_id", id) }.ToArray(),
           (AssetBalanceQuery[] balanceQuery) =>
           {
               onSuccess(mapAssetBalances(balanceQuery));
           },
           (string error) => { });
        }

        public static IEnumerator GetByAccountAndAssetId(string accountId, string assetId, Blockchain blockchain, Action<AssetBalance> onSuccess)
        {
            yield return blockchain.Query<AssetBalanceQuery>("ft3.get_asset_balance", new List<(string, object)>() {
                ("account_id", accountId),
                ("asset_id", assetId)
            }.ToArray(),
           (AssetBalanceQuery balanceQuery) =>
           {
               onSuccess(mapAssetBalance(balanceQuery));
           },
           (string error) => { });
        }

        public static IEnumerator GiveBalance(string accountId, string assetId, int amount, Blockchain blockchain, Action onSuccess)
        {
            var request = blockchain.Connection.NewTransaction(new byte[][] { }, (string error) => { UnityEngine.Debug.Log(error); });
            request.AddOperation("ft3.dev_give_balance", assetId, accountId, amount);
            yield return request.PostAndWait(onSuccess);
        }

        public struct AssetBalanceQuery
        {
            public string id;
            public string name;
            public long amount;
            public string chain_id;
        }

        public static AssetBalance[] mapAssetBalances(AssetBalanceQuery[] balanceQuery)
        {
            List<AssetBalance> assetsBalances = new List<AssetBalance>();
            foreach (var item in balanceQuery)
            {
                assetsBalances.Add(mapAssetBalance(item));
            }

            return assetsBalances.ToArray();
        }

        public static AssetBalance mapAssetBalance(AssetBalanceQuery balanceQuery)
        {
            return new AssetBalance(
                balanceQuery.amount,
                new Asset(balanceQuery.name, balanceQuery.chain_id)
            );
        }
    }
}