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
    private string _scheme = "unity";
    private string _tempRawTx = "temp_raw_tx";

    private void Awake()
    {
        AotHelper.EnsureList<AssetBalance.AssetBalanceQuery>();
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
    }

    private void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        bool isBatchMode = false;
        for (int i = 0; i < args.Length; i++) {
            if(args[i].StartsWith(this._scheme)) 
            {
                FileIOWrapper.SetString(_tempRawTx, args[i]);
            }

            isBatchMode = isBatchMode || args[i].StartsWith("-batchmode");
        }

        if(isBatchMode) Application.Quit();
        
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
        
        while(!FileIOWrapper.HasKey(_tempRawTx))
        {
            yield return new WaitForSeconds(1);
        }

        var payload = FileIOWrapper.GetString(_tempRawTx);
        FileIOWrapper.DeleteKey(_tempRawTx);
        
        payload = payload.Split("?"[0])[1];
        PanelManager.Instance.AccountIdText.text = payload;

        var raw = payload.Split("="[0])[1];

        yield return sso.FinalizeLogin(raw, ((Account, User) ac) =>
        {
            PanelManager.Instance.AccountIdText.text = ac.Item1.Id;
        });
    }

    public void Connect()
    {
        if(this._blockchain == null) return;
        StartCoroutine(SSOS());
    }

    public void Register()
    {
        Testing.Register();
    }
}
