using System;
using System.Text;
using System.Collections;
using System.Web;

namespace Chromia.Postchain.Ft3
{
    public class SSO
    {
        public readonly Blockchain Blockchain;
        public readonly SSOStore Store;
        private static string _vaultUrl = "https://vault-testnet.chromia.com";

        public SSO(Blockchain blockchain, SSOStore store = null)
        {
            this.Blockchain = blockchain;

            if (store == null)
            {
                Store = new SSOStoreDefault();
            }
            else
            {
                Store = store;
            }
        }

        public static string VaultUrl
        {
            get => _vaultUrl;
            set { _vaultUrl = value; }
        }

        private IEnumerator GetAccountAndUserByStoredIds<T>(Action<(Account, User)> onSuccess)
        {
            var keyPair = this.Store.KeyPair;
            var accountID = this.Store.AccountID;

            if (keyPair == null || accountID == null) onSuccess((null, null));

            var authDescriptor = new SingleSignatureAuthDescriptor(
                keyPair.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            );

            var user = new User(
                keyPair, authDescriptor
            );

            Account account = null;
            yield return this.Blockchain.NewSession(user).GetAccountById<T>(accountID,
                (Account _account) =>
                {
                    account = _account;
                }
            );

            if (account == null || user == null) onSuccess((null, null));
            onSuccess((account, user));
        }

        public IEnumerator AutoLogin<T>(Action<(Account, User)> onSuccess)
        {
            Account account = null;
            User user = null;

            yield return GetAccountAndUserByStoredIds<T>(
                ((Account, User) au) =>
                {
                    account = au.Item1;
                    user = au.Item2;
                }
            );

            if (account == null || user == null) onSuccess((null, null));

            bool isAuthDescriptorValid = false;

            yield return account.IsAuthDescriptorValid(user.AuthDescriptor.ID,
                (bool isValid) => { isAuthDescriptorValid = isValid; }
            );

            if (!isAuthDescriptorValid) onSuccess((null, null));
            onSuccess((account, user));
        }

        public void InitiateLogin(string successUrl, string cancelUrl)
        {
            this.Store.Clear();
            KeyPair keyPair = new KeyPair();
            this.Store.TmpPrivKey = keyPair.PrivKey;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(
                "{0}/?route=/authorize&dappId={1}&pubkey={2}&successAction={3}&cancelAction={4}&version=0.1",
                SSO._vaultUrl, this.Blockchain.Id, Util.ByteArrayToString(keyPair.PubKey), new Uri(successUrl), new Uri(cancelUrl)
            );
            UnityEngine.Application.OpenURL(sb.ToString());

        }

        public IEnumerator PendingSSO(Action<(Account, User)> onSuccess)
        {
            var url = UnityEngine.Application.absoluteURL;
            var uri = new Uri(url);
            var queryString = uri.GetComponents(UriComponents.Query, UriFormat.Unescaped);
            var components = HttpUtility.ParseQueryString(queryString);
            var tx = components["rawTx"];

            if (tx != null)
            {
                yield return FinalizeLogin(tx, onSuccess);
            }
        }

        public IEnumerator FinalizeLogin(string tx, Action<(Account, User)> onSuccess)
        {
            var keyPair = this.Store.TmpKeyPair;
            this.Store.ClearTmp();

            if (keyPair == null) throw new Exception("Error loading public key");

            this.Store.PrivKey = keyPair.PrivKey;

            var authDescriptor = new SingleSignatureAuthDescriptor(
                keyPair.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            );

            var user = new User(
                keyPair,
                authDescriptor
            );

            // tx FROM RAW Sign
            // validateTransaction(transaction, keyPair.pubKey);
            // post tx

            // let accountId = getAccountId(transaction);

            // this.store.accountId = accountId;

            // const account = await this.blockchain.newSession(user).getAccountById(accountId);

            // return [account, user];
            yield return null;
        }

        public IEnumerator Logout<T>()
        {
            Account account = null;
            User user = null;

            yield return GetAccountAndUserByStoredIds<T>(
                ((Account, User) au) =>
                {
                    account = au.Item1;
                    user = au.Item2;
                }
            );

            if (account != null && user != null)
            {
                yield return account.DeleteAuthDescriptor<T>(user.AuthDescriptor, () => { });
            }

            this.Store.Clear();
        }
    }
}
