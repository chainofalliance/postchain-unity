using System.Collections;
using System;

using Chromia.Postchain.Client.Unity;

namespace Chromia.Postchain.Ft3
{
    public class BlockchainSession
    {
        public readonly User User;
        public readonly Blockchain Blockchain;

        public BlockchainSession(User user, Blockchain blockchain)
        {
            this.User = user;
            this.Blockchain = blockchain;
        }

        public IEnumerator GetAccountById<T>(string id, Action<Account> onSuccess)
        {
            yield return Account.GetById<T>(id, this, onSuccess);
        }

        public IEnumerator GetAccountsByParticipantId(string id, Action<Account[]> onSuccess)
        {
            yield return Account.GetByParticipantId(id, this, onSuccess);
        }

        public IEnumerator GetAccountsByAuthDescriptorId(string id, Action<Account[]> onSuccess)
        {
            yield return Account.GetByAuthDescriptorId(id, this, onSuccess);
        }

        public PostchainTransaction NewTransaction()
        {
            return this.Blockchain.Connection.NewTransaction(
                new byte[][] { this.User.KeyPair.PubKey },
                (string error) => { UnityEngine.Debug.Log(error); });
        }

        public IEnumerator Query<T>(string queryName, (string name, object content)[] queryObject, Action<T> onSuccess, Action<string> onError)
        {
            return this.Blockchain.Query<T>(queryName, queryObject, onSuccess, onError);
        }

        public IEnumerator Call<T>(Operation operation, Action onSuccess)
        {
            return this.Blockchain.Call<T>(operation, this.User, onSuccess);
        }
    }
}