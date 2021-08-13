using Chromia.Postchain.Ft3;
using System.Collections;
using System;

public class BlockchainUtil
{
    const string CHAINID = "1A3A5B4C919798B52292094185B37E71898CC245FA9F0AC51A33B473150FE889";
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
