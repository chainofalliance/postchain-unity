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

        public Blockchain(string id, BlockchainInfo info, BlockchainClient connection, DirectoryService directoryService)
        {
            this.Id = id;
            this.Info = info;
            this.Connection = connection;
            this._directoryService = directoryService;
        }

        public static IEnumerator Initialize<T>(string blockchainRID, DirectoryService directoryService, Action<Blockchain> onSuccess, Action<string> onError)
        {
            var chainConnectionInfo = directoryService.GetChainConnectionInfo(blockchainRID);

            if (chainConnectionInfo == null)
            {
                throw new Exception("Cannot find details for chain with RID: " + blockchainRID);
            }

            GameObject goConnection = new GameObject();
            goConnection.AddComponent<BlockchainClient>();

            BlockchainClient connection = goConnection.GetComponent<BlockchainClient>();
            connection.Setup(
                blockchainRID,
                chainConnectionInfo.Url
            );

            Blockchain blockchain = null;
            yield return BlockchainInfo.GetInfo<BlockchainInfo>(connection,
            (BlockchainInfo info) =>
            {
                blockchain = new Blockchain(blockchainRID, info, connection, directoryService);
            },
            (string error) =>
            {
                blockchain = new Blockchain(blockchainRID, new BlockchainInfo(connection.BlockchainRID, null, null, 0, 0), connection, directoryService);
            });

            onSuccess(blockchain);
        }

        public BlockchainSession NewSession(User user)
        {
            return new BlockchainSession(user, this);
        }

        // public async Task<Account[]> GetAccountsByParticipantId(byte[] id, User user)
        // {
        //     return await Account.GetByParticipantId(id, this.NewSession(user));
        // }

        // public async Task<Account[]> GetAccountsByAuthDescriptorId(byte[] id, User user)
        // {
        //     return await Account.GetByAuthDescriptorId(id, this.NewSession(user));
        // }

        // public async Task<Account> RegisterAccount(AuthDescriptor authDescriptor, User user)
        // {
        //     return await Account.Register(authDescriptor, this.NewSession(user));
        // }

        // public async Task<Asset[]> GetAssetsByName(string name)
        // {
        //     return await Asset.GetByName(name, this);
        // }

        // public async Task<Asset> GetAssetById(byte[] id)
        // {
        //     return await Asset.GetById(id, this);
        // }

        // public async Task<Asset[]> GetAllAssets()
        // {
        //     return await Asset.GetAssets(this);
        // }

        // public async Task LinkChain(byte[] chainId)
        // {
        //     var tx = this.Connection.Gtx.NewTransaction(new byte[][] { });
        //     tx.AddOperation("ft3.xc.link_chain", Util.ByteArrayToString(chainId));
        //     await tx.PostAndWaitConfirmation();
        // }

        // public async Task<bool> IsLinkedWithChain(byte[] chainId)
        // {
        //     var info = await this.Query<int>(
        //         "ft3.xc.is_linked_with_chain",
        //         ("chain_rid", Util.ByteArrayToString(chainId))
        //     );

        //     if (info.control.Error)
        //     {
        //         return false;
        //     }

        //     return info.content == 1;
        // }

        public IEnumerator GetLinkedChainsIds<T>(Action<string[]> onSuccess, Action<string> onError)
        {
            return this.Query<string[]>("ft3.xc.get_linked_chains", new List<(string, object)>().ToArray(), onSuccess, onError);
        }

        public IEnumerator GetLinkedChains<T>(Action<List<Blockchain>> onSuccess, Action<string> onError)
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
                yield return Blockchain.Initialize<Blockchain>(chaindId, this._directoryService,
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

        // public async Task<PostchainErrorControl> Call(Operation operation, User user)
        // {
        //     var txBuilder = this.CreateTransactionBuilder();
        //     txBuilder.AddOperation(operation);
        //     var tx = txBuilder.Build(user.AuthDescriptor.Signers);
        //     tx.Sign(user.KeyPair);
        //     return await tx.Post();
        // }

        public TransactionBuilder CreateTransactionBuilder()
        {
            return new TransactionBuilder(this);
        }
    }
}