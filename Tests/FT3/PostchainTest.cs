using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class PostchainTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }
    private void DefaultErrorHandler(string error) { }

    // should instantiate blockchain by passing chain id as a string
    [UnityTest]
    public IEnumerator PostchainTestRun1()
    {
        string CHAINID = "849AD8C9AC720A21962187D0BDA6168DA274E1D17D39AAD513559171FDDC6914";
        string NODEURL = "http://localhost:7740";

        Blockchain blockchain = null;
        Postchain postchain = new Postchain(NODEURL);
        yield return postchain.Blockchain(CHAINID, (Blockchain _blockchain) => blockchain = _blockchain, DefaultErrorHandler);

        Assert.NotNull(blockchain);
    }
}
