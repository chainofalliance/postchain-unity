using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using System;
using UnityEngine;

public class SSOStandalone : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;

    private Blockchain _blockchain;

    private void Awake()
    {
        AotHelper.EnsureList<AssetBalance.AssetBalanceQuery>();
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
    }

    private void Start()
    {
        StartCoroutine(ConnectBlockchain());
    }

    private IEnumerator ConnectBlockchain()
    {
        Postchain postchain = new Postchain(_baseURL);
        yield return postchain.Blockchain<Blockchain>(_blockchainRID,
        (Blockchain _blockchain) =>
        {
            this._blockchain = _blockchain;
        }, (string error) => { Debug.Log(error); });
    }

    private IEnumerator SSOS()
    {
        SSO sso = new SSO(this._blockchain, new SSOStoreLocalStorage());
        SSO.VaultUrl = _vaultUrl;
        sso.InitiateLogin(_successUrl, _cancelUrl);
        var dlInstance = ProcessDeepLink.Instance;
        while (dlInstance != null && dlInstance.DeepLinkURL == "[none]")
        {
            yield return new WaitForSeconds(1);
        }

        var pairs = SSO.GetParams(dlInstance.Payload);
        if (pairs.ContainsKey("rawTx"))
        {
            var raw = pairs["rawTx"];
            yield return sso.FinalizeLogin(raw, ((Account, User) ac) =>
            {
                PanelManager.Instance.AccountIdText.text = ac.Item1.Id;
            });
        }
    }

    public void Connect() 
    {
        if(this._blockchain == null) return;
        StartCoroutine(SSOS());
    }
}
