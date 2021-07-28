using Cryptography.ECDSA;
using System;
using System.Collections;

using UnityEngine;

namespace Chromia.Postchain.Client.Unity
{
    public class TxStatusResponse
    {
        public string status = "";
        public string rejectReason = "";
    }

    public class PostchainTransaction
    {
        public bool sent = false;
        public bool error = false;
        public string errorMessage = "";

        internal Gtx GtxObject;
        private string _baseUrl;
        private string _brid;
        private Action<string> _onError;


        internal PostchainTransaction(Gtx gtx, string baseUrl, string brid, Action<string> onError)
        {
            this.GtxObject = gtx;
            this._baseUrl = baseUrl;
            this._brid = brid;
            this._onError = onError;
        }

        ///<summary>
        ///Add an operation to the Transaction.
        ///</summary>
        ///<param name = "name">Name of the operation.</param>
        ///<param name = "args">Array of object parameters. For example {"Hamburg", 42}</param>
        public void AddOperation(string name, params object[] args)
        {
            this.GtxObject.AddOperationToGtx(name, args);
        }

        ///<summary>
        ///Commit the Transaction and send it to the blockchain.
        ///</summary>
        ///<returns>Task, which returns null if it was succesful or the error message if not.</returns>
        public IEnumerator Post()
        {
            if (this.sent)
            {
                Debug.LogError("Tried to send tx twice");
            }
            else
            {
                var payload = String.Format(@"{{""tx"": ""{0}""}}", Encode());

                var request = new PostchainRequestRaw(this._baseUrl, "tx/" + this._brid);
                yield return request.Post(payload);

                this.sent = true;
                this.error = request.error;
                this.errorMessage = request.errorMessage;

                if (this.error)
                {
                    this._onError(this.errorMessage);
                }
            }
        }
        public IEnumerator PostAndWait(Action callback)
        {
            yield return Post();
            yield return WaitConfirmation();
            
            if (this.error)
            {
                this._onError(this.errorMessage);
            }
            else
            {
                callback();
            }
        }

        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] pub = pubKey;
            if(pubKey == null)
            {
                pub = Secp256K1Manager.GetPublicKey(privKey, true);
            }
            this.GtxObject.Sign(privKey, pub);
        }

        private IEnumerator WaitConfirmation()
        {
            var request = new PostchainRequest<TxStatusResponse>(this._baseUrl, "tx/" + this._brid + "/" + GetTxRID() + "/status");
            yield return request.Get();

            var ret = request.parsedContent;
            this.error = true;
            switch(ret.status)
            {
                case "confirmed":
                    this.error = false;
                    this.errorMessage = "";
                    break;
                case "rejected":
                case "unknown":
                    this.errorMessage = ret.rejectReason;
                    break;
                case "waiting":
                    yield return new WaitForSeconds(0.511f);
                    yield return WaitConfirmation();
                    break;
                case "exception":
                    this.errorMessage = "HTTP Exception: " + ret.rejectReason;
                    break;
                default:
                    this.errorMessage = "Got unexpected response from server: " + ret.status;
                    break;
            }
        }
        
        private string GetTxRID()
        {
            return PostchainUtil.ByteArrayToString(this.GetBufferToSign());
        }

        private byte[] GetBufferToSign()
        {
            return this.GtxObject.GetBufferToSign();
        }
        
        private void AddSignature(byte[] pubKey, byte[] signature)
        {
            this.GtxObject.AddSignature(pubKey, signature);
        }

        private string Encode()
        {
            return this.GtxObject.Serialize();
        }
    }
}