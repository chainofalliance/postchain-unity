using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

public class FileHub : MonoBehaviour
{
    private static IEnumerator GetChunkDataByHash(FileChain fileChain, string hash,
        Action<string> onSuccess, Action<string> onError)
    {
        yield return fileChain.GetChunkDataByHash(hash, onSuccess, onError);
    }

    [SerializeField]
    private string nodeUrl;

    [SerializeField]
    private string brid;

    [SerializeField]
    private GameObject fileChainContainer;
    public Blockchain Blockchain { get; private set; }

    private void Start()
    {
        StartCoroutine(Establish(this.nodeUrl, this.brid));
    }

    public IEnumerator Establish(string nodeUrl = null, string brid = null)
    {
        if (nodeUrl != null && brid != null)
        {
            Postchain postchain = new Postchain(nodeUrl);
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
        //[OK] File already stored possible
        yield return ExecuteOperation(
            user,
            Operation.Op("fs.allocate_file", user.AuthDescriptor.ID, file.Hash, file.Size),
            () => Debug.Log("file allocated"), onError
        );

        FileChainLocation[] fileChainLocations = null;
        yield return GetFileLocation(file.Hash,
            (FileChainLocation[] _locations) => fileChainLocations = _locations, onError);

        foreach (var fileChainLocation in fileChainLocations)
        {
            yield return this.StoreChunks(user, file, fileChainLocation, onError);
        }

        onSuccess();
    }

    /**
    * Retrieves a file by its hash.
    *
    * @param passphrase optional options for retrieving file.
    */
    public IEnumerator GetFile(byte[] hash, Action<FsFile> onSuccess, Action<string> onError)
    {
        FileChainLocation[] fileChainLocations = null;
        yield return GetFileLocation(hash,
            (FileChainLocation[] _locations) => fileChainLocations = _locations, onError);

        ChunkHashIndex[] chunkHashes = null;
        yield return this.ExecuteQuery<ChunkHashIndex[]>("fs.get_file_chunks",
            new (string, object)[] { ("file_hash", Util.ByteArrayToString(hash)) },
            (ChunkHashIndex[] _chunkHashes) => chunkHashes = _chunkHashes, onError);

        var fileChain = this.InitFileChainClient(fileChainLocations[0]);
        var chunkIndexes = new List<ChunkIndex>();

        foreach (var chunkHash in chunkHashes)
        {
            yield return this.GetChunk(fileChain, chunkHash,
                (ChunkIndex _chunkIndex) => chunkIndexes.Add(_chunkIndex), onError);
        }

        if (chunkIndexes.Count > 0)
        {
            var file = FsFile.FromChunks(chunkIndexes);
            onSuccess(file);
        }

        Destroy(fileChain.gameObject);
    }

    private FileChain InitFileChainClient(FileChainLocation fileChainLocation)
    {
        UnityEngine.Debug.Log("Initializing filechain client with brid " + fileChainLocation.Brid);
        return FileChain.Create(fileChainLocation.Location, fileChainLocation.Brid, this.fileChainContainer);
    }

    private IEnumerator StoreChunks(User user, FsFile file,
        FileChainLocation fileChainLocation, Action<string> onError)
    {
        var fileChain = this.InitFileChainClient(fileChainLocation);

        for (int i = 0; i < file.NumberOfChunks(); i++)
        {
            yield return this.StoreChunk(
                user, fileChain, new ChunkIndex(file.GetChunk(i), i),
                file.Hash, onError);
        }

        Destroy(fileChain.gameObject);
    }

    private IEnumerator StoreChunk(User user, FileChain fileChain, ChunkIndex chunkIndex,
        byte[] hash, Action<string> onError)
    {
        var chunkToStore = chunkIndex.Data;
        var chunkAllocated = false;
        yield return this.AllocateChunk(user, chunkToStore, hash, chunkIndex.Index,
            () => chunkAllocated = true);

        if (chunkAllocated)
            yield return this.PersistChunkDataInFilechain(user, fileChain, chunkToStore, onError);
    }

    private IEnumerator PersistChunkDataInFilechain(User user, FileChain fileChain,
        byte[] data, Action<string> onError)
    {
        yield return fileChain.StoreChunkData(user, data, onError);
    }

    private IEnumerator GetFileLocation(byte[] hash, Action<FileChainLocation[]> onSuccess, Action<string> onError)
    {
        yield return this.ExecuteQuery<FileChainLocation[]>("fs.get_file_location",
            new (string, object)[] { ("file_hash", Util.ByteArrayToString(hash)) },
            (FileChainLocation[] locations) =>
            {
                if (locations.Length < 1) throw new Exception("Did not receive enough active & online Filechains");
                onSuccess(locations);
            },
            onError);
    }

    private IEnumerator GetChunk(FileChain fileChain, IChunkHashIndex chunkHash,
        Action<ChunkIndex> onSuccess, Action<string> onError)
    {
        yield return FileHub.GetChunkDataByHash(fileChain, chunkHash.Hash,
            (string data) =>
            {
                onSuccess(new ChunkIndex(Util.HexStringToBuffer(data), chunkHash.Index));
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
