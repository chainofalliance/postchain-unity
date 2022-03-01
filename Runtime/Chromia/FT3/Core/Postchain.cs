using Chromia.Postchain.Client.Unity;
using System.Collections;
using System;

namespace Chromia.Postchain.Ft3
{
    public class Postchain
    {
        private readonly string _url;

        public Postchain(string url)
        {
            this._url = url;
        }

        public IEnumerator Blockchain(string id, Action<Blockchain> onSuccess, Action<string> onError)
        {
            var directoryService = new DirectoryServiceBase(
                new ChainConnectionInfo[] { new ChainConnectionInfo(id, _url) }
            );

            yield return Ft3.Blockchain.Initialize(id, directoryService, onSuccess, onError);
        }

        public IEnumerator Blockchain(int chainId, Action<Blockchain> onSuccess, Action<string> onError)
        {
            var request = new PostchainRequestRaw(_url, "brid/iid_" + chainId);

            yield return request.Get();

            if (request.error)
            {
                UnityEngine.Debug.LogError("InitializeBRIDFromChainID: " + request.errorMessage);
            }
            else
            {
                var id = request.content;
                yield return Blockchain(id, onSuccess, onError);
            }
        }
    }
}
