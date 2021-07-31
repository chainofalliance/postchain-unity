using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using Chromia.Postchain.Ft3;
using Chromia.Postchain.Client;

using UnityEngine;

public class FT3DemoScript : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;

    private Blockchain _blockchain;

    private void Start()
    {
        ChainConnectionInfo connectionInfo = new ChainConnectionInfo(
            _blockchainRID,
            _baseURL
        );

        FakeDirectoryService fakeDirectoryService = new FakeDirectoryService(new List<ChainConnectionInfo>() { connectionInfo }.ToArray());


        var enumerator = Blockchain.Initialize<Blockchain>(_blockchainRID, fakeDirectoryService,
        (Blockchain blockchain) =>
        {
            Debug.Log(blockchain.Info.Name);
            _blockchain = blockchain;
            StartCoroutine(Workflow1());
        },
        (string error) =>
        {
            Debug.Log(error);
        });

        StartCoroutine(enumerator);
    }

    private IEnumerator RegisterAccount(Action<Account> onSuccess)
    {
        KeyPair keyPair = new KeyPair();
        SingleSignatureAuthDescriptor singleSignatureAuthDescriptor = new SingleSignatureAuthDescriptor(
            keyPair.PubKey,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray(),
            null
        );
        User user = new User(keyPair, singleSignatureAuthDescriptor);
        BlockchainSession session = _blockchain.NewSession(user);

        return Account.Register<Account>(user.AuthDescriptor, session, onSuccess);
    }

    private IEnumerator FindAssetByName(string name, Action<Asset[]> onSuccess)
    {
        return Asset.GetByName(name, this._blockchain, (Asset[] assets) => { onSuccess(assets); });
    }

    public IEnumerator Workflow1()
    {
        Account account = null;
        Asset asset = null;
        yield return RegisterAccount((Account _account) => { account = _account; });
        yield return Asset.Register<Asset>(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), _blockchain, (Asset _asset) => { asset = _asset; });
        // yield return AssetBalance.GiveBalance(account.Id, asset.Id, 100, _blockchain, () => { });
        yield return AssetBalance.GetByAccountId(account.Id, _blockchain, (AssetBalance[] balances) => { Debug.Log(balances[0].Asset.Name); });
    }
}

public class FakeDirectoryService : DirectoryService
{
    private readonly ChainConnectionInfo[] _chainInfos;

    public FakeDirectoryService(ChainConnectionInfo[] chainInfos)
    {
        this._chainInfos = chainInfos;
    }

    public ChainConnectionInfo GetChainConnectionInfo(string id)
    {
        return this._chainInfos.ToList().Find(info => info.ChainId.Equals(id));
    }
}

public class TestUtil
{
    public static System.Random _random = new System.Random();

    public static int GenerateNumber(int max = 10000)
    {
        return _random.Next(0, System.Int32.MaxValue);
    }

    public static string GenerateAssetName(string prefix = "UNITY")
    {
        return prefix + "_" + GenerateNumber();
    }

    public static string GenerateId()
    {
        return Util.ByteArrayToString(PostchainUtil.Sha256(
            BitConverter.GetBytes(GenerateNumber())
        ));
    }

    public static byte[] BlockchainAccountId(byte[] chainId)
    {
        var gtv = new List<dynamic>(){
            "B",
            chainId
            }.ToArray();

        return PostchainUtil.HashGTV(gtv);
    }
}