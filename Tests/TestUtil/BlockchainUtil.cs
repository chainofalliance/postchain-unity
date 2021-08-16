using Chromia.Postchain.Ft3;
using System.Collections;
using System;

public class BlockchainUtil
{
    const string CHAINID = "EB9326199CE87797116BF6019EE5234D5AF2C51C2F45E4098544DC8B83F97D3D";
    const string NODEURL = "http://localhost:7740";

    public static IEnumerator GetDefaultBlockchain(Action<Blockchain> onSuccess)
    {
        yield return Blockchain.Initialize(
            CHAINID,
            DirectoryServiceUtil.GetDefaultDirectoryService(CHAINID, NODEURL),
            onSuccess,
            (string error) => { throw new Exception(error); }
        );
    }
}
