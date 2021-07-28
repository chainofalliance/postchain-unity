using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public dynamic[] ToGTV()
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
        byte[] ID
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
        List<byte[]> PubKey
        {
            get;
        }
        byte[] Hash();
        dynamic[] ToGTV();
    }

    public class Account
    {
        // private Payme<ntHistorySyncManager _paymentHistorySyncManager = new PaymentHistorySyncManager();
        public readonly byte[] Id;
        public List<AuthDescriptor> AuthDescriptor;
        public List<AssetBalance> Assets = new List<AssetBalance>();
        public RateLimit RateLimit;
        public readonly BlockchainSession Session;

        public Account(byte[] id, AuthDescriptor[] authDescriptor, BlockchainSession session)
        {
            this.Id = id;
            this.AuthDescriptor = authDescriptor.ToList();
            this.Session = session;
        }

        public byte[] GetID()
        {
            return Id;
        }

        public Blockchain GetBlockchain()
        {
            return this.Session.Blockchain;
        }

        // public static async Task<Account[]> GetByParticipantId(byte[] id, BlockchainSession session)
        // {
        //     // var gtv = AccountQueries.AccountsByParticipantId(id);
        //     (dynamic content, PostchainErrorControl control) accountIds = await session.Query<dynamic>("ft3.get_accounts_by_participant_id", ("id", Util.ByteArrayToString(id)));


        //     var idList = new List<byte[]>();
        //     foreach (var accountId in accountIds.content)
        //     {
        //         idList.Add(Util.HexStringToBuffer((string)accountId));
        //     }

        //     return await Account.GetByIds(idList, session);
        // }

        // public static async Task<Account[]> GetByAuthDescriptorId(byte[] id, BlockchainSession session)
        // {
        //     // var gtv = AccountQueries.AccountsByAuthDescriptorId(id);
        //     (dynamic content, PostchainErrorControl control) accountIds = await session.Query<dynamic>("ft3.get_accounts_by_auth_descriptor_id", ("descriptor_id", Util.ByteArrayToString(id)));

        //     var idList = new List<byte[]>();
        //     foreach (var accountId in accountIds.content)
        //     {
        //         idList.Add(Util.HexStringToBuffer((string)accountId));
        //     }

        //     return await Account.GetByIds(idList, session);
        // }

        // public static async Task<Account> Register(AuthDescriptor authDescriptor, BlockchainSession session)
        // {
        //     PostchainErrorControl opControl = await session.Call(AccountDevOperations.Register(authDescriptor));
        //     if (opControl.Error)
        //     {
        //         return null;
        //     }
        //     var account = new Account(authDescriptor.Hash(), new List<AuthDescriptor> { authDescriptor }.ToArray(), session);
        //     await account.Sync();
        //     return account;
        // }

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

        // public static async Task<Account[]> GetByIds(List<byte[]> ids, BlockchainSession session)
        // {
        //     var accounts = new List<Account>();
        //     foreach (var id in ids)
        //     {
        //         var account = await Account.GetById(id, session);
        //         accounts.Add(account);
        //     }

        //     return accounts.ToArray();
        // }

        // public static async Task<Account> GetById(byte[] id, BlockchainSession session)
        // {
        //     // var gtv = AccountQueries.AccountById(id);
        //     (dynamic content, PostchainErrorControl control) account = await session.Query<dynamic>("ft3.get_account_by_id", ("id", Util.ByteArrayToString(id)));

        //     if (account.control.Error)
        //     {
        //         return null;
        //     }

        //     var acc = new Account(id, new List<AuthDescriptor>().ToArray(), session);
        //     await acc.Sync();
        //     return acc;
        // }

        // public async Task<PostchainErrorControl> AddAuthDescriptor(AuthDescriptor authDescriptor)
        // {
        //     var response = await this.Session.Call(AccountOperations.AddAuthDescriptor(
        //         this.Id,
        //         this.Session.User.AuthDescriptor.ID,
        //         authDescriptor)
        //     );
        //     if (!response.Error)
        //     {
        //         this.AuthDescriptor.Add(authDescriptor);
        //     }
        //     return response;
        // }

        // public async Task DeleteAllAuthDescriptorsExclude(AuthDescriptor authDescriptor)
        // {
        //     await this.Session.Call(AccountOperations.DeleteAllAuthDescriptorsExclude(
        //         this.Id,
        //         authDescriptor.ID)
        //     );
        //     this.AuthDescriptor.Clear();
        //     this.AuthDescriptor.Add(authDescriptor);
        // }

        // public async Task<PostchainErrorControl> DeleteAuthDescriptor(AuthDescriptor authDescriptor)
        // {
        //     var opControl = await this.Session.Call(AccountOperations.DeleteAuthDescriptor(
        //         this.Id,
        //         this.Session.User.AuthDescriptor.ID,
        //         authDescriptor.ID)
        //     );
        //     await this.SyncAuthDescriptors();

        //     return opControl;
        // }

        // public async Task Sync()
        // {
        //     await SyncAssets();
        //     await SyncAuthDescriptors();
        //     await SyncRateLimit();
        // }

        // private async Task SyncAssets()
        // {
        //     this.Assets = await AssetBalance.GetByAccountId(this.Id, this.Session.Blockchain);
        // }

        // private async Task SyncAuthDescriptors()
        // {
        //     // var authGtv = AccountQueries.AccountAuthDescriptors(this.Id);
        //     (dynamic content, PostchainErrorControl control) authDescriptors = await this.Session.Query<dynamic>("ft3.get_account_auth_descriptors", ("id", Util.ByteArrayToString(this.Id)));

        //     var authDescriptorFactory = new AuthDescriptorFactory();
        //     List<AuthDescriptor> authList = new List<AuthDescriptor>();

        //     foreach (var authDescriptor in authDescriptors.content)
        //     {
        //         authList.Add(
        //             authDescriptorFactory.Create(
        //                 Util.StringToAuthType((string)authDescriptor["type"]),
        //                 Util.HexStringToBuffer((string)authDescriptor["args"])
        //             )
        //         );
        //     }

        //     this.AuthDescriptor = authList;
        // }

        // private async Task SyncRateLimit()
        // {
        //     this.RateLimit = await RateLimit.GetByAccountRateLimit(this.Id, this.Session.Blockchain);
        // }

        // public AssetBalance GetAssetById(byte[] id)
        // {
        //     return this.Assets.Find(assetBalance => Util.ByteArrayToString(assetBalance.Asset.GetId()).Equals(Util.ByteArrayToString(id)));
        // }

        // public async Task<PostchainErrorControl> TransferInputsToOutputs(dynamic[] inputs, dynamic[] outputs)
        // {
        //     var transactionBuilder = this.GetBlockchain().CreateTransactionBuilder();

        //     transactionBuilder.AddOperation(AccountOperations.Transfer(inputs, outputs));
        //     transactionBuilder.AddOperation(AccountOperations.Nop());
        //     var tx = transactionBuilder.BuildAndSign(this.Session.User);
        //     PostchainErrorControl opControl = await tx.Post();
        //     await this.SyncAssets();
        //     return opControl;
        // }

        // public async Task<PostchainErrorControl> Transfer(byte[] accountId, byte[] assetId, int amount)
        // {
        //     var input = new List<dynamic>{
        //         this.Id,
        //         assetId,
        //         this.Session.User.AuthDescriptor.ID,
        //         amount,
        //         new dynamic[] {}
        //     }.ToArray();

        //     var output = new List<dynamic>{
        //         accountId,
        //         assetId,
        //         amount,
        //         new dynamic[] {}
        //     }.ToArray();

        //     return await this.TransferInputsToOutputs(
        //         new List<dynamic>() { input }.ToArray(),
        //         new List<dynamic>() { output }.ToArray()
        //     );
        // }

        // public async Task BurnTokens(byte[] assetId, int amount)
        // {
        //     var input = new List<dynamic>(){
        //         this.Id,
        //         assetId,
        //         this.Session.User.AuthDescriptor.Hash(),
        //         amount,
        //         new dynamic[] {}
        //     }.ToArray();

        //     await this.TransferInputsToOutputs(
        //         new List<dynamic>() { input }.ToArray(),
        //         new List<dynamic>() { }.ToArray()
        //     );
        // }

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