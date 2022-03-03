using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

public class SSOAndroid : MonoBehaviour
{
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;

    private Blockchain _blockchain;
    private SSO _sso;

    public string deeplinkURL;

    private void Awake()
    {
        SSO.VaultUrl = _vaultUrl;

        Application.deepLinkActivated += onDeepLinkActivated;
        if (!string.IsNullOrEmpty(Application.absoluteURL))
            onDeepLinkActivated(Application.absoluteURL);
        else
            deeplinkURL = "[none]";
    }

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private void onDeepLinkActivated(string url)
    {
        deeplinkURL = url;
        var payload = SSO.GetParams(url);

        if (payload.ContainsKey("rawTx"))
        {
            var raw = payload["rawTx"];
            StartCoroutine(HandlePayload(raw));
        }
    }

    private IEnumerator HandlePayload(string raw)
    {
        yield return _sso.FinalizeLogin(raw, PanelManager.AddOptionToPanel, DefaultErrorHandler);
    }

    private IEnumerator StartRoutine()
    {
        yield return ConnectToBlockchain();
        _sso = new SSO(this._blockchain, new SSOStoreLocalStorage());

        yield return _sso.AutoLogin(PanelManager.AddOptionsToPanel, DefaultErrorHandler);
    }

    private IEnumerator ConnectToBlockchain()
    {
        Postchain postchain = new Postchain(_baseURL);
        yield return postchain.Blockchain(1, (Blockchain _blockchain) => this._blockchain = _blockchain, DefaultErrorHandler);
    }

    public void Connect()
    {
        if (this._blockchain == null) return;
        _sso.InitiateLogin(_successUrl, _cancelUrl);
    }

    private void DefaultErrorHandler(string error)
    {
        throw new Exception(error);
    }
}