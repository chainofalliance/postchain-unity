using Chromia.Postchain.Ft3;
using System.Collections;
using System;

public class BlockchainUtil
{
    const string CHAINID = "849AD8C9AC720A21962187D0BDA6168DA274E1D17D39AAD513559171FDDC6914";
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
