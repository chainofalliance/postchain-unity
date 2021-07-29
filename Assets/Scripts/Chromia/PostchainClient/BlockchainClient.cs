using System;
using System.Collections;

using Chromia.Postchain.Client.Unity;

using UnityEngine;

namespace Chromia.Postchain.Client
{

    public class BlockchainClient : MonoBehaviour
    {
        [SerializeField] private string _blockchainRID;
        [SerializeField] private int _chainId;
        [SerializeField] private string _baseURL;

        void Start()
        {
            if (String.IsNullOrEmpty(this._blockchainRID))
            {
                StartCoroutine(InitializeBRIDFromChainID());
            }
        }

        public void Setup(string blockchainRID, string baseURL)
        {
            this._blockchainRID = blockchainRID;
            this._baseURL = baseURL;
        }

        ///<summary>
        ///Create a new Transaction.
        ///</summary>
        ///<param name = "signers">Array of signers (can be null).</param>
        ///<returns>New Transaction object.</returns>
        public PostchainTransaction NewTransaction(byte[][] signers, Action<string> onError)
        {
            Gtx newGtx = new Gtx(this._blockchainRID);

            foreach(byte[] signer in signers)
            {
                newGtx.AddSignerToGtx(signer);
            }
          
            return new PostchainTransaction(newGtx, this._baseURL, this._blockchainRID, onError);
        }

        public IEnumerator Query<T>(string queryName, (string name, object content)[] queryObject, Action<T> onSuccess, Action<string> onError)
        {
            var request = new PostchainQuery<T>(this._baseURL, this._blockchainRID);

            yield return request.Query(queryName, queryObject);

            if (request.error)
            {
                onError(request.errorMessage);
            }
            else
            {
                onSuccess(request.content);
            }
        }

        private IEnumerator InitializeBRIDFromChainID()
        {
            var request = new PostchainRequestRaw(this._baseURL, "brid/iid_" + this._chainId);

            yield return request.Get();

            if (request.error)
            {
                Debug.LogError("InitializeBRIDFromChainID: " + request.errorMessage);
            }
            else
            {
                this._blockchainRID = request.content;
            }
        }
    }
}
