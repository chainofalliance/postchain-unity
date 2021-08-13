using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class BlockchainTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { }

    // should provide info
    [UnityTest]
    public IEnumerator BlockchainTestRun1()
    {
        yield return SetupBlockchain();
        yield return BlockchainInfo.GetInfo(blockchain.Connection,
        (BlockchainInfo info) => { Assert.AreEqual(info.Name, "Unity FT3"); }, DefaultErrorHandler);
    }
}
