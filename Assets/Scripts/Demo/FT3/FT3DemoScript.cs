using System.Collections.Generic;
using System.Linq;

using Chromia.Postchain.Ft3;

using UnityEngine;

public class FT3DemoScript : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;

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
            // Register User
        },
        (string error) =>
        {
            Debug.Log(error);
        });

        StartCoroutine(enumerator);
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