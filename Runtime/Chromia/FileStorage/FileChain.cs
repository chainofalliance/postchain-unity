using Chromia.Postchain.Client;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

public class FileChain : MonoBehaviour
{
    public static FileChain Create(string nodeApiUrl, string brid, GameObject container)
    {
        GameObject fileChainGO = new GameObject();
        fileChainGO.name = "FileChain" + brid;
        if (container != null) fileChainGO.transform.SetParent(container.transform);

        FileChain fileChain = fileChainGO.AddComponent<FileChain>();
        BlockchainClient connection = fileChainGO.AddComponent<BlockchainClient>();

        connection.Setup(
            brid,
            nodeApiUrl
        );

        fileChain.Client = connection;
        fileChain.Brid = brid;

        return fileChain;
    }

    public BlockchainClient Client;
    public string Brid;

    public IEnumerator StoreChunkData(User user, byte[] data, Action<string> onError)
    {
        //[OK] Chunk already stored possible
        var hash = Util.ByteArrayToString(PostchainUtil.Sha256(data));

        var tx = this.Client.NewTransaction(user.AuthDescriptor.Signers.ToArray(), onError);
        tx.AddOperation("fs.add_chunk_data", data);
        var nop = AccountOperations.Nop();
        tx.AddOperation(nop.Name, nop.Args);
        tx.Sign(user.KeyPair.PrivKey, user.KeyPair.PubKey);

        yield return tx.PostAndWait(() => Debug.LogFormat("Stored data for hash: {0}, in filechain: {1}", hash, this.Brid));
    }

    public IEnumerator ChunkHashExists(string hash, Action<bool> onSuccess, Action<string> onError)
    {
        yield return this.Client.Query<bool>("fs.chunk_hash_exists",
            new (string, object)[] { ("hash", hash) }, onSuccess, onError);
    }

    public IEnumerator GetChunkDataByHash(string hash, Action<string> onSuccess, Action<string> onError)
    {
        Debug.LogFormat("Retrieving chunk data by hash {0} from filechain: {1}", hash, this.Brid);

        yield return this.Client.Query<string>("fs.get_chunk",
            new (string, object)[] { ("hash", hash) }, onSuccess, onError);
    }
}
