using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.Unity;

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

        private IEnumerator GetAccountAndUserByStoredIds(Action<(Account, User)> onSuccess)
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
            yield return this.Blockchain.NewSession(user).GetAccountById(accountID,
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

            yield return GetAccountAndUserByStoredIds(
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

        // For Webgl
        public IEnumerator PendingSSO(Action<(Account, User)> onSuccess, Action onDiscard)
        {
            var url = UnityEngine.Application.absoluteURL;
            var pairs = GetParams(url);

            if (pairs.ContainsKey("rawTx"))
            {
                var raw = pairs["rawTx"];
                yield return FinalizeLogin(raw, onSuccess);
            }
            else
            {
                onDiscard();
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

            var gtx = PostchainUtil.DeserializeGTX(Util.HexStringToBuffer(tx));
            gtx.Sign(keyPair.PrivKey, keyPair.PubKey);

            var connection = this.Blockchain.Connection;
            var postchainTransaction = new PostchainTransaction(gtx, connection.BaseUrl, connection.BlockchainRID,
            (string error) => { UnityEngine.Debug.Log(error); });

            bool isSuccess = false;
            yield return postchainTransaction.PostAndWait(() =>
            {
                UnityEngine.Debug.Log("Success");
                isSuccess = true;
            });

            if (isSuccess)
            {
                var accountID = GetAccountId(gtx);
                this.Store.AccountID = accountID;

                Account account = null;
                yield return this.Blockchain.NewSession(user).GetAccountById(accountID, (Account _account) => account = _account);

                onSuccess((account, user));
            }
        }

        private string GetAccountId(Gtx gtx)
        {
            var ops = gtx.Operations;
            if (ops.Count == 1)
            {
                return ops[0].Args[0].String;
            }
            else if (ops.Count == 2)
            {
                return ops[1].Args[0].String;
            }
            else
            {
                throw new Exception("Invalid sso transaction");
            }
        }

        public IEnumerator Logout<T>()
        {
            Account account = null;
            User user = null;

            yield return GetAccountAndUserByStoredIds(
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

        public static Dictionary<string, string> GetParams(string uri)
        {
            var matches = Regex.Matches(uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value)
            );
        }
    }
}
