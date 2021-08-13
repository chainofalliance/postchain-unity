using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using UnityEngine;

namespace Chromia.Postchain.Ft3
{
    public class Blockchain
    {
        public readonly string Id;
        public readonly BlockchainInfo Info;
        public readonly BlockchainClient Connection;
        private readonly DirectoryService _directoryService;
        private System.Random _random;

        public Blockchain(string id, BlockchainInfo info, BlockchainClient connection, DirectoryService directoryService)
        {
            this.Id = id;
            this.Info = info;
            this.Connection = connection;
            this._directoryService = directoryService;
            _random = new System.Random();
        }

        public static IEnumerator Initialize(string blockchainRID, DirectoryService directoryService, Action<Blockchain> onSuccess, Action<string> onError)
        {
            var chainConnectionInfo = directoryService.GetChainConnectionInfo(blockchainRID);

            if (chainConnectionInfo == null)
            {
                throw new Exception("Cannot find details for chain with RID: " + blockchainRID);
            }

            GameObject goConnection = new GameObject();
            goConnection.name = "Blockchain_" + blockchainRID;
            BlockchainClient connection = goConnection.AddComponent<BlockchainClient>();

            connection.Setup(
                blockchainRID,
                chainConnectionInfo.Url
            );

            Blockchain blockchain = null;
            yield return BlockchainInfo.GetInfo<BlockchainInfo>(connection,
            (BlockchainInfo info) =>
            {
                blockchain = new Blockchain(blockchainRID, info, connection, directoryService);
            }, onError);

            onSuccess(blockchain);
        }

        public BlockchainSession NewSession(User user)
        {
            return new BlockchainSession(user, this);
        }

        public IEnumerator GetAccountsByParticipantId(string id, User user, Action<Account[]> onSuccess)
        {
            yield return Account.GetByParticipantId(id, this.NewSession(user), onSuccess);
        }

        public IEnumerator GetAccountsByAuthDescriptorId(string id, User user, Action<Account[]> onSuccess)
        {
            yield return Account.GetByAuthDescriptorId(id, this.NewSession(user), onSuccess);
        }

        public IEnumerator RegisterAccount(AuthDescriptor authDescriptor, User user, Action<Account> onSuccess)
        {
            yield return Account.Register(authDescriptor, this.NewSession(user), onSuccess);
        }

        public IEnumerator GetAssetsByName(string name, Action<Asset[]> onSuccess)
        {
            yield return Asset.GetByName(name, this, onSuccess);
        }

        public IEnumerator GetAssetById(string id, Action<Asset> onSuccess)
        {
            yield return Asset.GetById(id, this, onSuccess);
        }

        public IEnumerator GetAllAssets(Action<Asset[]> onSuccess)
        {
            yield return Asset.GetAssets(this, onSuccess);
        }

        public IEnumerator LinkChain(string chainId, Action onSuccess)
        {
            var request = this.Connection.NewTransaction(new byte[][] { }, (string error) => { UnityEngine.Debug.Log(error); });
            request.AddOperation("ft3.xc.link_chain", chainId);
            yield return request.PostAndWait(onSuccess);
        }

        public IEnumerator IsLinkedWithChain(string chainId, Action<bool> onSuccess)
        {
            yield return this.Query<int>("ft3.xc.is_linked_with_chain", new List<(string, object)>() { ("chain_rid", chainId) }.ToArray(),
            (int is_linked) => { onSuccess(is_linked == 1); },
            (string error) => { });
        }

        public IEnumerator GetLinkedChainsIds<T>(Action<string[]> onSuccess, Action<string> onError)
        {
            return this.Query<string[]>("ft3.xc.get_linked_chains", new List<(string, object)>().ToArray(), onSuccess, onError);
        }

        public IEnumerator GetLinkedChains(Action<List<Blockchain>> onSuccess, Action<string> onError)
        {
            List<string> chaindIds = null;

            yield return this.GetLinkedChainsIds<string[]>(
            (string[] linkedChains) =>
            {
                chaindIds = linkedChains.ToList<string>();
            },
            (string error) =>
            {
                chaindIds = new List<string>();
            });

            List<Blockchain> blockchains = new List<Blockchain>();

            foreach (var chaindId in chaindIds)
            {
                yield return Blockchain.Initialize(chaindId, this._directoryService,
                (Blockchain blockchain) =>
                {
                    blockchains.Add(blockchain);
                },
                (string error) =>
                {
                    Debug.Log(error);
                });
            }

            onSuccess(blockchains);
        }

        public IEnumerator Query<T>(string queryName, (string name, object content)[] queryObject, Action<T> onSuccess, Action<string> onError)
        {
            return this.Connection.Query<T>(queryName, queryObject, onSuccess, onError);
        }

        public IEnumerator Call(Operation operation, User user, Action onSuccess)
        {
            KeyPair keyPair = user.KeyPair;
            var request = this.Connection.NewTransaction(
                new byte[][] { keyPair.PubKey },
                (string error) => { Debug.Log(error); });

            request.AddOperation(operation.Name, operation.Args);
            request.AddOperation("nop", _random.Next().ToString());

            request.Sign(keyPair.PrivKey, keyPair.PubKey);

            yield return request.PostAndWait(onSuccess);
        }
    }
}