using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SSOStandalone : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;

    private Blockchain _blockchain;
    private SSO _sso;
    public static string SCHEME = "unity";
    private string _tempRawTx = "temp_raw_tx";

    private void Awake()
    {
        AotHelper.EnsureList<AssetBalance.AssetBalanceQuery>();
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
    }

    private void Start()
    {
#if UNITY_STANDALONE
        string[] args = System.Environment.GetCommandLineArgs();
        SSOStore store = new SSOStoreLocalStorage();
        store.Load();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith(SCHEME))
            {
                store.TmpTx = args[i];
                store.Save();

                Application.Quit();
            }
        }
        if (Application.isBatchMode) Application.Quit();
#endif
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return ConnectBlockchain();
        _sso = new SSO(this._blockchain, new SSOStoreLocalStorage());
        SSO.VaultUrl = _vaultUrl;
        yield return _sso.AutoLogin<List<(Account, User)>>((List<(Account, User)> aus) =>
        {
            var options = aus.Select((elem) => new Dropdown.OptionData(elem.Item1.Id));
            PanelManager.Instance.AccountsDropdown.AddOptions(options.ToList());
        });
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
        _sso.InitiateLogin(_successUrl, _cancelUrl);

#if UNITY_STANDALONE
        var counter = 0;
        while (_sso.Store.TmpTx == null)
        {
            yield return new WaitForSeconds(5);
            PanelManager.Instance.AccountIdText.text = (counter++).ToString();
            _sso.Store.Load();
        }

        var payload = _sso.Store.TmpTx;
        payload = payload.Split("?"[0])[1];
        PanelManager.Instance.AccountIdText.text = payload;

        var raw = payload.Split("="[0])[1];

        yield return _sso.FinalizeLogin(raw, ((Account, User) ac) =>
        {
            PanelManager.Instance.AccountIdText.text = ac.Item1.Id;
        });
#endif
    }

    public void Connect()
    {
        if (this._blockchain == null) return;
        StartCoroutine(SSOS());
    }

    public void Register()
    {
        Testing.RegisterUnix();
    }
}
