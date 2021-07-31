using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace Chromia.Postchain.Ft3
{
    public enum AuthType
    {
        None,
        SingleSig,
        MultiSig
    }

    public enum FlagsType
    {
        None,
        Account,
        Transfer
    }

    public class Flags
    {
        private List<FlagsType> FlagsOrder = new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer };
        public List<FlagsType> FlagList;

        public Flags(List<FlagsType> flags)
        {
            this.FlagList = flags;
        }

        public bool HasFlag(FlagsType flag)
        {
            return this.FlagList.Contains(flag);
        }

        public object[] ToGTV()
        {
            var validFlags = new List<string>();
            foreach (var flag in this.FlagList)
            {
                if (this.FlagsOrder.Contains(flag))
                {
                    validFlags.Add(Util.FlagTypeToString(flag));
                }
            }

            return validFlags.ToArray();
        }
    }

    public interface AuthDescriptor
    {
        string ID
        {
            get;
        }
        List<byte[]> Signers
        {
            get;
        }
        IAuthdescriptorRule Rule
        {
            get;
        }
        byte[] Hash();
        object[] ToGTV();
    }

    public class Account
    {
        // readonly paymentHistorySyncManager = new PaymentHistorySyncManager();
        public readonly string Id;
        public List<AuthDescriptor> AuthDescriptor;
        public List<AssetBalance> Assets = new List<AssetBalance>();
        public RateLimit RateLimit;
        public readonly BlockchainSession Session;

        public Account(string id, AuthDescriptor[] authDescriptor, BlockchainSession session)
        {
            this.Id = id;
            this.AuthDescriptor = authDescriptor.ToList();
            this.Session = session;
        }

        public string GetID()
        {
            return Id;
        }

        public Blockchain GetBlockchain()
        {
            return this.Session.Blockchain;
        }

        public static IEnumerator GetByParticipantId(string id, BlockchainSession session, Action<Account[]> onSuccess)
        {
            List<string> accountIDs = null;
            yield return session.Query<string[]>("ft3.get_accounts_by_participant_id", new List<(string, object)>() { ("id", id) }.ToArray(),
            (string[] _accountIDs) =>
            {
                accountIDs = _accountIDs.ToList();
            },
            (string error) => { });

            yield return Account.GetByIds(accountIDs, session, onSuccess);
        }

        public static IEnumerator GetByAuthDescriptorId(string id, BlockchainSession session, Action<Account[]> onSuccess)
        {
            List<string> accountIDs = null;
            yield return session.Query<string[]>("ft3.get_accounts_by_auth_descriptor_id", new List<(string, object)>() { ("id", id) }.ToArray(),
            (string[] _accountIDs) =>
            {
                accountIDs = _accountIDs.ToList();
            },
            (string error) => { });

            yield return Account.GetByIds(accountIDs, session, onSuccess);
        }

        public static IEnumerator Register<T>(AuthDescriptor authDescriptor, BlockchainSession session, Action<Account> onSuccess)
        {
            Account account = null;
            yield return session.Call<T>(AccountDevOperations.Register(authDescriptor),
            () =>
                {
                    account = new Account(
                        Util.ByteArrayToString(authDescriptor.Hash()),
                        new List<AuthDescriptor> { authDescriptor }.ToArray(),
                        session);
                }
            );

            if (account != null)
            {
                yield return account.Sync(() => { });
                onSuccess(account);
            }
        }

        // public static byte[] RawRegisterTransaction(AuthDescriptor authDescriptor, AuthDescriptor ssoAuthDescriptor, BlockchainSession session)
        // {
        //     TransactionBuilder txBuilder = session.Blockchain.CreateTransactionBuilder();
        //     List<byte[]> signers = new List<byte[]>();
        //     txBuilder.AddOperation(AccountDevOperations.Register(authDescriptor));
        //     txBuilder.AddOperation(AccountOperations.AddAuthDescriptor(authDescriptor.ID, authDescriptor.ID, ssoAuthDescriptor));

        //     signers.AddRange(authDescriptor.Signers);
        //     // TODO Add sso signers
        //     var tx = txBuilder.Build(signers);
        //     tx.Sign(session.User.KeyPair);
        //     return tx.Raw();
        // }
        // JS
        // static rawTransactionRegister(user: User, authDescriptor: AuthDescriptor, blockchain: Blockchain) : Buffer {
        // return blockchain.transactionBuilder()
        //     .add(register(user.authDescriptor))
        //     .add(addAuthDescriptor(user.authDescriptor.id, user.authDescriptor.id, authDescriptor))
        //     .build([user.authDescriptor.signers, authDescriptor.signers].flat())
        //     .sign(user.keyPair)
        //     .raw()
        // }
        // static rawTransactionAddAuthDescriptor(
        // accountId: Buffer,
        // user: User,
        // authDescriptor: AuthDescriptor,
        // blockchain: Blockchain
        // ): Buffer {
        //     return blockchain.transactionBuilder()
        //         .add(addAuthDescriptor(accountId, user.authDescriptor.id, authDescriptor))
        //         .build([user.authDescriptor.signers , authDescriptor.signers].flat())
        //         .sign(user.keyPair)
        //         .raw()
        // }

        public static IEnumerator GetByIds(List<string> ids, BlockchainSession session, Action<Account[]> onSuccess)
        {
            var accounts = new List<Account>();
            foreach (var id in ids)
            {
                yield return Account.GetById(id, session,
                (Account account) =>
                {
                    accounts.Add(account);
                });
            }

            onSuccess(accounts.ToArray());
        }

        public static IEnumerator GetById(string id, BlockchainSession session, Action<Account> onSuccess)
        {
            Account account = null;
            yield return session.Query<string>("ft3.get_account_by_id", new List<(string, object)>() { ("id", id) }.ToArray(),
            (string _id) =>
            {
                account = new Account(_id, new List<AuthDescriptor>().ToArray(), session);
            },
            (string error) => { });

            if (account != null)
            {
                yield return account.Sync(null);
                onSuccess(account);
            }
        }
        public IEnumerator AddAuthDescriptor<T>(AuthDescriptor authDescriptor, Action onSuccess)
        {
            yield return this.Session.Call<T>(AccountOperations.AddAuthDescriptor(
                this.Id,
                this.Session.User.AuthDescriptor.ID,
                authDescriptor),
                () =>
                {
                    this.AuthDescriptor.Add(authDescriptor);
                    onSuccess();
                }
            );
        }

        public IEnumerator IsAuthDescriptorValid(string id, Action<bool> onSuccess)
        {
            yield return Session.Query<bool>("ft3.is_auth_descriptor_valid",
                new (string, object)[] { ("account_id", this.Id), ("auth_descriptor_id", id) },
                onSuccess,
                (string error) =>
                {
                    UnityEngine.Debug.Log(error);
                }
            );
        }

        public IEnumerator DeleteAllAuthDescriptorsExclude<T>(AuthDescriptor authDescriptor, Action onSuccess)
        {
            yield return this.Session.Call<T>(AccountOperations.DeleteAllAuthDescriptorsExclude(
                this.Id,
                authDescriptor.ID),
                () =>
                {
                    this.AuthDescriptor.Clear();
                    this.AuthDescriptor.Add(authDescriptor);
                    onSuccess();
                }
            );
        }

        public IEnumerator DeleteAuthDescriptor<T>(AuthDescriptor authDescriptor, Action onSuccess)
        {
            yield return this.Session.Call<T>(AccountOperations.DeleteAuthDescriptor(
                this.Id,
                this.Session.User.AuthDescriptor.ID,
                authDescriptor.ID),
                () =>
                {
                    SyncAuthDescriptors();
                    onSuccess();
                }
            );
        }

        public IEnumerator Sync(Action onSuccess)
        {
            UnityEngine.Debug.Log("SyncAssets");
            yield return SyncAssets();
            UnityEngine.Debug.Log("SyncAuthDescriptors");
            yield return SyncAuthDescriptors();
            UnityEngine.Debug.Log("SyncRateLimit");
            yield return SyncRateLimit();
        }

        private IEnumerator SyncAssets()
        {
            yield return AssetBalance.GetByAccountId(this.Id, this.Session.Blockchain,
                (AssetBalance[] balances) => { this.Assets = balances.ToList(); }
            );
        }
        private IEnumerator SyncAuthDescriptors()
        {
            AuthDescriptorFactory.AuthDescriptorQuery[] authDescriptors = null;

            yield return this.Session.Query<AuthDescriptorFactory.AuthDescriptorQuery[]>("ft3.get_account_auth_descriptors", new List<(string, object)>() {
                ("id", this.Id)
            }.ToArray(),
            (AuthDescriptorFactory.AuthDescriptorQuery[] authQuery) =>
            {
                authDescriptors = authQuery;
            },
            (string error) => { });


            var authDescriptorFactory = new AuthDescriptorFactory();
            List<AuthDescriptor> authList = new List<AuthDescriptor>();

            foreach (var authDescriptor in authDescriptors)
            {
                authList.Add(
                    authDescriptorFactory.Create(
                        Util.StringToAuthType((string)authDescriptor.type),
                        Util.HexStringToBuffer((string)authDescriptor.args)
                    )
                );
            }

            this.AuthDescriptor = authList;
        }

        private IEnumerator SyncRateLimit()
        {
            yield return RateLimit.GetByAccountRateLimit(this.Id, this.Session.Blockchain,
                (RateLimit rateLimit) => { this.RateLimit = rateLimit; }
            );
        }

        public AssetBalance GetAssetById(string id)
        {
            return this.Assets.Find(assetBalance => assetBalance.Asset.Id.Equals(id));
        }

        public IEnumerator TransferInputsToOutputs<T>(object[] inputs, object[] outputs, Action onSuccess)
        {
            yield return this.Session.Call<T>(AccountOperations.Transfer(inputs, outputs), onSuccess);
            yield return this.SyncAssets();
        }

        public IEnumerator Transfer<T>(string accountId, string assetId, long amount, Action onSuccess)
        {
            var input = new List<object>{
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.ID,
                amount,
                new object[] {}
            }.ToArray();

            var output = new List<object>{
                accountId,
                assetId,
                amount,
                new object[] {}
            }.ToArray();

            yield return this.TransferInputsToOutputs<T>(
                new List<object>() { input }.ToArray(),
                new List<object>() { output }.ToArray(),
                onSuccess
            );
        }

        public IEnumerator BurnTokens<T>(string assetId, long amount, Action onSuccess)
        {
            var input = new List<object>(){
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.Hash(),
                amount,
                new object[] {}
            }.ToArray();

            yield return this.TransferInputsToOutputs<T>(
                new List<object>() { input }.ToArray(),
                new List<object>() { }.ToArray(),
                onSuccess
            );
        }

        // public async Task<PaymentHistoryEntryShort[]> GetPaymentHistory()
        // {
        //     return await PaymentHistory.GetAccountById(this.Id, this.Session.Blockchain.Connection);
        // }

        // public async Task<PaymentHistoryIterator> GetPaymentHistoryIterator(int pageSize)
        // {
        //     if (pageSize < 1)
        //     {
        //         throw new Exception("Page size has to be greater than 1");
        //     }
        //     await this._paymentHistorySyncManager.SyncAccount(this.Id, this.Session.Blockchain);
        //     return this._paymentHistorySyncManager.PaymentHistoryStore.GetIterator(this.Id, pageSize);
        // }

        // public async Task XcTransfer(byte[] destinationChainId, byte[] destinationAccountId, byte[] assetId, int amount)
        // {
        //     var transactionBuilder = this.GetBlockchain().CreateTransactionBuilder();
        //     transactionBuilder.AddOperation(XcTransferOp(destinationChainId, destinationAccountId, assetId, amount));
        //     transactionBuilder.AddOperation(AccountOperations.Nop());
        //     var tx = transactionBuilder.BuildAndSign(this.Session.User);
        //     await tx.Post();
        //     await this.SyncAssets();
        // }

        // /* Operation and query */
        // public Operation XcTransferOp(byte[] destinationChainId, byte[] destinationAccountId, byte[] assetId, int amount)
        // {

        //     var source = new List<dynamic>() {
        //         this.Id,
        //         assetId,
        //         this.Session.User.AuthDescriptor.ID,
        //         amount,
        //         new dynamic[] {}
        //     }.ToArray();

        //     var target = new List<dynamic>() {
        //         destinationAccountId,
        //         new dynamic[] {}
        //     }.ToArray();

        //     var hops = new List<byte[]>(){
        //         destinationChainId
        //     }.ToArray();

        //     return AccountOperations.XcTransfer(source, target, hops);
        // }
    }
}