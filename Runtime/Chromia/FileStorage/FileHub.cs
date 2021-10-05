using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

namespace Chromia.Postchain.Fs
{
    public class FileHub : MonoBehaviour
    {
        private static IEnumerator GetChunkDataByHash(FileChain fileChain, string hash,
            Action<string> onSuccess, Action<string> onError)
        {
            yield return fileChain.GetChunkDataByHash(hash, onSuccess, onError);
        }

        [SerializeField] private string nodeUrl;
        [SerializeField] private string brid;
        [SerializeField] private GameObject fileChainContainer;

        public Blockchain Blockchain { get; private set; }

        private void Start()
        {
            StartCoroutine(Establish(this.nodeUrl, this.brid));
        }

        public IEnumerator Establish(string nodeUrl = null, string brid = null)
        {
            if (nodeUrl != null && brid != null)
            {
                Chromia.Postchain.Ft3.Postchain postchain = new Chromia.Postchain.Ft3.Postchain(nodeUrl);
                yield return postchain.Blockchain(brid,
                    (Blockchain _blockchain) => this.Blockchain = _blockchain,
                    (string error) => throw new System.Exception(error));
            }
        }

        /**
        * Stores a file. Contacts the Filehub and allocates a chunk, and then persists the data in the correct filechain.
        */
        public IEnumerator StoreFile(User user, FsFile file, Action onSuccess, Action<string> onError)
        {
            //[OK] File already stored possible needs to be catched
            yield return ExecuteOperation(
                user,
                Operation.Op("fs.allocate_file", user.AuthDescriptor.ID, file.Hash, file.Size),
                () => Debug.Log("file allocated"), onError
            );

            yield return this.StoreChunks(user, file, onError);
            onSuccess();
        }
        /**
        * Retrieves a file by its hash.
        *
        * @param passphrase optional options for retrieving file.
        */
        public IEnumerator GetFile(byte[] hash, Action<FsFile> onSuccess, Action<string> onError)
        {
            ChunkLocation[] fileChainLocations = null;
            yield return GetChunkLocations(hash,
                (ChunkLocation[] _locations) => fileChainLocations = _locations, onError);

            var chunkIndexes = new List<ChunkIndex>();
            foreach (var chunkLocation in fileChainLocations)
            {
                Debug.LogFormat("Getting chunk {0} from filechain {1}", chunkLocation.Hash, chunkLocation.Location);
                var fileChain = this.InitFileChainClient(chunkLocation.Location, chunkLocation.Brid);

                yield return this.GetChunk(fileChain, chunkLocation,
                    (ChunkIndex _chunkIndex) => chunkIndexes.Add(_chunkIndex), onError);
            }

            if (chunkIndexes.Count > 0)
            {
                var file = FsFile.FromChunks(chunkIndexes);
                onSuccess(file);
            }
        }

        private FileChain InitFileChainClient(string url, string brid)
        {
            Debug.Log("Initializing filechain client with brid " + brid);
            return FileChain.Create(url, brid, this.fileChainContainer);
        }

        private IEnumerator StoreChunks(User user, FsFile file, Action<string> onError)
        {
            Debug.LogFormat("Storing nr of chunks: {0}", file.NumberOfChunks());
            for (int i = 0; i < file.NumberOfChunks(); i++)
            {
                yield return this.AllocateChunk(user, file.GetChunk(i), file.Hash, i, () => { });
            }

            ChunkLocation[] filechainLocations = null;
            yield return this.GetChunkLocations(file.Hash,
                (ChunkLocation[] _locations) => filechainLocations = _locations, onError);

            foreach (var chunkLocation in filechainLocations)
            {
                Debug.LogFormat("Storing chunk {0} in filechain {1}", chunkLocation.Hash, chunkLocation.Location);
                var fileChain = this.InitFileChainClient(chunkLocation.Location, chunkLocation.Brid);

                yield return this.PersistChunkDataInFilechain(user, fileChain, file.GetChunk(chunkLocation.Idx), onError);
            }
        }

        private IEnumerator PersistChunkDataInFilechain(User user, FileChain fileChain,
            byte[] data, Action<string> onError)
        {
            yield return fileChain.StoreChunkData(user, data, onError);
        }

        private IEnumerator GetChunkLocations(byte[] hash, Action<ChunkLocation[]> onSuccess, Action<string> onError)
        {
            yield return this.ExecuteQuery<ChunkLocation[]>("fs.get_chunk_locations",
                new (string, object)[] { ("file_hash", Util.ByteArrayToString(hash)) },
                (ChunkLocation[] locations) =>
                {
                    Debug.LogFormat("Got number of chunks: {0}", locations.Length);

                    if (locations.Length < 1) throw new Exception("Did not receive enough active & online Filechains");
                    onSuccess(locations);
                },
                onError);
        }

        private IEnumerator GetChunk(FileChain fileChain, ChunkLocation chunkLocation,
            Action<ChunkIndex> onSuccess, Action<string> onError)
        {
            yield return FileHub.GetChunkDataByHash(fileChain, chunkLocation.Hash,
                (string data) =>
                {
                    onSuccess(new ChunkIndex(Util.HexStringToBuffer(data), chunkLocation.Idx));
                }, onError
            );
        }

        private IEnumerator AllocateChunk(User user, byte[] chunk, byte[] fileHash,
            int index, Action onSuccess)
        {
            var hash = Chromia.Postchain.Client.PostchainUtil.Sha256(chunk);

            var op = Operation.Op(
                "fs.allocate_chunk",
                user.AuthDescriptor.ID,
                fileHash,
                hash,
                chunk.Length,
                index
            );
            /* Ok error should be catched */
            yield return ExecuteOperation(user, op, onSuccess, (string error) => Debug.LogWarning(error));
        }

        /**
        * Executes a operation towards the Filehub.
        *
        * @param user to sign the operation.
        * @param operation to perform.
        * @param on success callback.
        * @param on error callback.
        */
        private IEnumerator ExecuteOperation(User user, Operation operation,
            Action onSuccess, Action<string> onError, bool addNop = false)
        {
            var builder = this.Blockchain.TransactionBuilder().Add(operation);

            if (addNop) builder.Add(AccountOperations.Nop());

            yield return builder.BuildAndSign(user, onError).PostAndWait(onSuccess);
        }

        /**
        * Queries the Filehub for data.
        *
        * @param query the identifier of the query.
        * @param data to provide in the query.
        * @param on success callback.
        * @param on error callback.
        */
        private IEnumerator ExecuteQuery<T>(string queryName,
            (string name, object content)[] queryObject,
            Action<T> onSuccess, Action<string> onError)
        {
            yield return this.Blockchain.Query<T>(queryName, queryObject, onSuccess, onError);
        }
    }
}