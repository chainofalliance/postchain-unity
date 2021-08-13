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
                Store = new SSOStoreLocalStorage();
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

        private IEnumerator GetAccountAndUserByStoredIds(Action<List<(Account, User)>> onSuccess)
        {
            List<(Account, User)> aus = new List<(Account, User)>();
            List<SavedSSOAccount> accounts = this.Store.GetAccounts();

            foreach (var acc in accounts)
            {
                yield return GetAccountAndUserByStoredId(acc, ((Account, User) au) =>
                {
                    aus.Add(au);
                });
            }

            onSuccess(aus);
        }

        private IEnumerator GetAccountAndUserByStoredId(SavedSSOAccount savedAccount, Action<(Account, User)> onSuccess)
        {
            var keyPair = new KeyPair(savedAccount.__ssoPrivKey);
            var authDescriptor = new SingleSignatureAuthDescriptor(
                keyPair.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            );

            var user = new User(
                keyPair, authDescriptor
            );

            Account account = null;
            yield return this.Blockchain.NewSession(user).GetAccountById(savedAccount.__ssoAccountId,
                (Account _account) =>
                {
                    account = _account;
                }
            );

            if (account != null)
            {
                yield return account.IsAuthDescriptorValid(user.AuthDescriptor.ID,
                    (bool isValid) =>
                    {
                        if (isValid) onSuccess((account, user));
                    }
                );
            }
        }

        public IEnumerator AutoLogin(Action<List<(Account, User)>> onSuccess)
        {
            yield return GetAccountAndUserByStoredIds(onSuccess);
        }

        public void InitiateLogin(string successUrl, string cancelUrl)
        {
            KeyPair keyPair = new KeyPair();
            this.Store.TmpPrivKey = keyPair.PrivKey;
            this.Store.Save();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(
                "{0}/?route=/authorize&dappId={1}&pubkey={2}&successAction={3}&cancelAction={4}&version=0.1",
                SSO._vaultUrl, this.Blockchain.Id, Util.ByteArrayToString(keyPair.PubKey), new Uri(successUrl), new Uri(cancelUrl)
            );
            UnityEngine.Application.OpenURL(sb.ToString());
        }

        public IEnumerator FinalizeLogin(string tx, Action<(Account, User)> onSuccess)
        {
            var keyPair = this.Store.TmpKeyPair;
            this.Store.ClearTmp();

            if (keyPair == null) throw new Exception("Error loading public key");

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
                this.Store.AddAccount(accountID, Util.ByteArrayToString(keyPair.PrivKey));
                this.Store.Save();

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

        public IEnumerator Logout<T>((Account, User) au)
        {
            yield return au.Item1.DeleteAuthDescriptor(au.Item2.AuthDescriptor, () => { });
            this.Store.ClearTmp();
            this.Store.Save();
        }

        public static Dictionary<string, string> GetParams(string uri)
        {
            var matches = Regex.Matches(uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value)
            );
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
    }
}
