using Chromia.Postchain.Ft3;
using System.Collections;
using System;

public class BlockchainUtil
{
    public static IEnumerator GetDefaultBlockchain(string chainId, string nodeUrl, Action<Blockchain> onSuccess)
    {
        yield return Blockchain.Initialize(
            chainId,
            DirectoryServiceUtil.GetDefaultDirectoryService(chainId, nodeUrl),
            onSuccess,
            (string error) => { throw new Exception(error); }
        );
    }
}
