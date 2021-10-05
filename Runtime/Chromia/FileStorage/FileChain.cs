using Chromia.Postchain.Client;
using Chromia.Postchain.Ft3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chromia.Postchain.Fs
{
    public class FileChain : MonoBehaviour
    {
        public static List<FileChain> FileChains = new List<FileChain>();

        public static FileChain GetFileChain(string brid)
        {
            return FileChains.Find(elem => elem.Brid.Equals(brid));
        }

        public static FileChain Create(string nodeApiUrl, string brid, GameObject container)
        {
            FileChain fileChain = GetFileChain(brid);

            if (fileChain == null)
            {
                GameObject fileChainGO = new GameObject();
                fileChainGO.name = "FileChain" + brid;
                if (container != null) fileChainGO.transform.SetParent(container.transform);

                fileChain = fileChainGO.AddComponent<FileChain>();
                BlockchainClient connection = fileChainGO.AddComponent<BlockchainClient>();

                connection.Setup(
                    brid,
                    nodeApiUrl
                );

                fileChain.Client = connection;
                fileChain.Brid = brid;

                FileChains.Add(fileChain);
            }

            return fileChain;
        }

        public BlockchainClient Client;
        public string Brid;

        public IEnumerator StoreChunkData(User user, byte[] data, Action<string> onError)
        {
            //[OK] Chunk already stored possible
            var tx = this.Client.NewTransaction(user.AuthDescriptor.Signers.ToArray(),
                (string error) =>
                {
                    // TODO: Check error message. If it's not a duplicate chunk, throw error.
                    // Error message is today not returned by the client.
                });
            tx.AddOperation("fs.add_chunk_data", Util.ByteArrayToString(data));
            var nop = AccountOperations.Nop();
            tx.AddOperation(nop.Name, nop.Args);
            tx.Sign(user.KeyPair.PrivKey, user.KeyPair.PubKey);

            yield return tx.PostAndWait(() => { });
        }

        public IEnumerator ChunkHashExists(string hash, Action<bool> onSuccess, Action<string> onError)
        {
            yield return this.Client.Query<bool>("fs.chunk_hash_exists",
                new (string, object)[] { ("hash", hash) }, onSuccess, onError);
        }

        public IEnumerator GetChunkDataByHash(string hash, Action<string> onSuccess, Action<string> onError)
        {
            yield return this.Client.Query<string>("fs.get_chunk",
                new (string, object)[] { ("hash", hash) }, onSuccess, onError);
        }
    }
}