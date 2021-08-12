using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SSOStandalone : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;
    [SerializeField] private string _customProtocolName;

    private Blockchain _blockchain;
    private SSO _sso;

    private void Awake()
    {
        ProtocolHandler.HandleTempTx(_customProtocolName);
        SSO.VaultUrl = _vaultUrl;

        AotHelper.EnsureList<AssetBalance.AssetBalanceQuery>();
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
    }

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return ConnectToBlockchain();
        _sso = new SSO(this._blockchain, new SSOStoreLocalStorage());

        yield return _sso.AutoLogin((List<(Account, User)> aus) =>
        {
            var options = aus.Select((elem) => new Dropdown.OptionData(elem.Item1.Id));
            PanelManager.Instance.AccountsDropdown.AddOptions(options.ToList());
        });
    }

    private IEnumerator ConnectToBlockchain()
    {
        Postchain postchain = new Postchain(_baseURL);
        yield return postchain.Blockchain<Blockchain>(_blockchainRID,
        (Blockchain _blockchain) =>
        {
            this._blockchain = _blockchain;
        }, (string error) =>
        {
            throw new Exception(error);
        });
    }

    private IEnumerator SSOS()
    {
        ProtocolHandler.Register(_customProtocolName);
        _sso.InitiateLogin(_successUrl, _cancelUrl);

        while (_sso.Store.TmpTx == null)
        {
            yield return new WaitForSeconds(3);
            _sso.Store.Load();
        }

        var payload = _sso.Store.TmpTx;
        payload = payload.Split("?"[0])[1];
        string raw = payload.Split("="[0])[1];

        yield return _sso.FinalizeLogin(raw, ((Account, User) ac) =>
        {
            var options = new Dropdown.OptionData[] { new Dropdown.OptionData(ac.Item1.Id) };
            PanelManager.Instance.AccountsDropdown.AddOptions(options.ToList());
        });
    }

    public void Connect()
    {
        if (this._blockchain == null) return;
        StartCoroutine(SSOS());
    }
}
