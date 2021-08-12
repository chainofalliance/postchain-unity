using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SSOWebgl : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;

    private Blockchain _blockchain;
    private SSO _sso;

    private void Awake()
    {
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

        yield return _sso.PendingSSO(
            ((Account, User) ac) =>
            {
            },
            () =>
            {

            }
        );

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

    public void Connect()
    {
        if (this._blockchain == null) return;
        _sso.InitiateLogin(_successUrl, _cancelUrl);
    }
}

