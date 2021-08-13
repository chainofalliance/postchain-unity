using System;
using System.Collections;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
    public class BlockchainInfo
    {
        public string Name;
        public string Website;
        public string Description;
        public RateLimitInfo RateLimitInfo;

        public BlockchainInfo(string name, string website, string description, RateLimitInfo rateLimitInfo)
        {
            this.Name = name;
            this.Website = website;
            this.Description = description;
            this.RateLimitInfo = rateLimitInfo;
        }
        public static IEnumerator GetInfo(BlockchainClient connection, Action<BlockchainInfo> onSuccess, Action<string> onError)
        {
            yield return connection.Query("ft3.get_blockchain_info", null,
            (BlockchainInfoQuery res) =>
            {
                var blockchainInfo = new BlockchainInfo(
                    res.name,
                    res.website,
                    res.description,
                    new RateLimitInfo(
                        res.rate_limit_active == 1,
                        res.rate_limit_max_points,
                        res.rate_limit_recovery_time,
                        res.rate_limit_points_at_account_creation
                    )
                );

                onSuccess(blockchainInfo);
            },
            (string error) =>
            {
                var blockchainInfo = new BlockchainInfo(
                    connection.BlockchainRID,
                    null,
                    null,
                    new RateLimitInfo(
                        false,
                        0,
                        0,
                        0
                    )
                );

                onSuccess(blockchainInfo);
                onError(error);
            });
        }

        public struct BlockchainInfoQuery
        {
            public string name;
            public string website;
            public string description;
            public int rate_limit_active;
            public int rate_limit_max_points;
            public int rate_limit_recovery_time;
            public int rate_limit_points_at_account_creation;
        }

    }
}