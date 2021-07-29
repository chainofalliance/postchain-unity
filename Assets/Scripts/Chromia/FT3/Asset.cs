using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class Asset
    {
        public string Name;
        public byte[] ChainId;

        public Asset(string name, byte[] chainId)
        {
            this.Name = name;
            this.ChainId = chainId;
        }

        public byte[] GetId()
        {
            var body = new List<dynamic>() { this.Name, this.ChainId };
            return PostchainUtil.HashGTV(body.ToArray());
        }

        // public static async Task<Asset> Register(string name, byte[] chainId, Blockchain blockchain)
        // {
        //     var tx = blockchain.Connection.Gtx.NewTransaction(new byte[][] {});
        //     tx.AddOperation("ft3.dev_register_asset", name, Util.ByteArrayToString(chainId));
        //     await tx.PostAndWaitConfirmation();
        //     return new Asset(name, chainId);
        // }

        // public static async Task<Asset[]> GetByName(string name, Blockchain blockchain)
        // {
        //     var assets = await blockchain.Connection.Gtx.Query<dynamic>("ft3.get_asset_by_name", ("name", name));
        //     List<Asset> assetList = new List<Asset>();

        //     foreach (var asset in assets.content)
        //     {
        //         assetList.Add(
        //             new Asset(
        //                 (string) asset["name"],
        //                 Util.HexStringToBuffer((string) asset["issuing_chain_rid"])
        //             )
        //         );
        //     }
        //     return assetList.ToArray();
        // }

        // public static async Task<Asset> GetById(byte[] id, Blockchain blockchain)
        // {
        //     var asset = await blockchain.Connection.Gtx.Query<dynamic>("ft3.get_asset_by_id", ("asset_id", Util.ByteArrayToString(id)));
        //     return new Asset((string) asset.content["name"], Util.HexStringToBuffer((string) asset.content["issuing_chain_rid"]));
        // }

        // public static async Task<Asset[]> GetAssets(Blockchain blockchain)
        // {
        //     var assets = await blockchain.Connection.Gtx.Query<dynamic>("ft3.get_all_assets");
        //     List<Asset> assetList = new List<Asset>();

        //     foreach (var asset in assets.content)
        //     {
        //         assetList.Add(
        //             new Asset(
        //                 (string) asset["name"],
        //                 Util.HexStringToBuffer((string) asset["issuing_chain_rid"])
        //             )
        //         );
        //     }
        //     return assetList.ToArray();
        // }
    }
}