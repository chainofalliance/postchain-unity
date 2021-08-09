using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class SSOStandalone : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl = "https://dev.vault.chromia-development.com";
    [SerializeField] private string _successUrl = "unitydl://";
    [SerializeField] private string _cancelUrl = "unitydl://";

    private UnityEvent dl_Event;

    private void Awake()
    {
        AotHelper.EnsureList<AssetBalance.AssetBalanceQuery>();
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
    }

    void Start()
    {
        dl_Event = new UnityEvent();
        StartCoroutine(SSOS());
    }

    public IEnumerator SSOS()
    {
        Postchain postchain = new Postchain(_baseURL);
        Blockchain blockchain = null;

        yield return postchain.Blockchain<Blockchain>(_blockchainRID,
        (Blockchain _blockchain) =>
        {
            blockchain = _blockchain;
        }, (string error) => { Debug.Log(error); });

        SSO sso = new SSO(blockchain, new SSOStoreLocalStorage());
        SSO.VaultUrl = _vaultUrl;

        sso.InitiateLogin(_successUrl, _cancelUrl);

        var dlInstance = ProcessDeepLink.Instance;
        while (dlInstance.DeepLinkURL == "[none]")
            yield return new WaitForSeconds(1);

        var pairs = SSO.GetParams(dlInstance.Payload);
        if (pairs.ContainsKey("rawTx"))
        {
            var raw = pairs["rawTx"];
            yield return sso.FinalizeLogin(raw, ((Account, User) ac) =>
           {
               Debug.Log("Registered");
               Debug.Log(ac.Item1.Id);
           });
        }
    }
}
