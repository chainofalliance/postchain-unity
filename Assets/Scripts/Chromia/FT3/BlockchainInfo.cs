using System;
using System.Collections;
using System.Collections.Generic;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
    public class BlockchainInfo
    {
        public string Name;
        public string Website;
        public string Description;
        public int RequestMaxCount;
        public int RequestRecoveryTime;

        public BlockchainInfo(string name, string website, string description, int requestMaxCount, int requestRecoveryTime)
        {
            this.Name = name;
            this.Website = website;
            this.Description = description;
            this.RequestMaxCount = requestMaxCount;
            this.RequestRecoveryTime = requestRecoveryTime;
        }
        public static IEnumerator GetInfo<BlockchainInfo>(BlockchainClient connection, Action<BlockchainInfo> onSuccess, Action<string> onError)
        {
            return connection.Query<BlockchainInfo>("ft3.get_blockchain_info", new List<(string, object)>().ToArray(), onSuccess, onError);
        }
    }
}