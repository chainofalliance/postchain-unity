using System.Collections;
using Chromia.Postchain.Ft3;
using Newtonsoft.Json.Utilities;
using UnityEngine;

public class SSODemoScript : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;

    private void Awake()
    {
        AotHelper.EnsureList<AssetBalance.AssetBalanceQuery>();
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
    }
    private void Start()
    {
        StartCoroutine(SSODemo());
    }

    public IEnumerator SSODemo()
    {
        Postchain postchain = new Postchain(_baseURL);
        Blockchain blockchain = null;

        yield return postchain.Blockchain<Blockchain>(_blockchainRID,
        (Blockchain _blockchain) =>
        {
            Debug.Log(_blockchain.Info.Name);
            blockchain = _blockchain;
        }, (string error) => { Debug.Log(error); });

        SSO sso = new SSO(blockchain, new SSOStoreLocalStorage());
        SSO.VaultUrl = "https://dev.vault.chromia-development.com";

        yield return sso.PendingSSO(
            ((Account, User) ac) =>
            {
                Debug.Log("Registered");
                Debug.Log(ac.Item1.Id);
            },
            () =>
            {
                sso.InitiateLogin("http://localhost:3000", "http://localhost:3000");
            }
        );
    }

}