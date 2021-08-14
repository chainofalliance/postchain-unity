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
        string CHAINID = "1A3A5B4C919798B52292094185B37E71898CC245FA9F0AC51A33B473150FE889";
        string NODEURL = "http://localhost:7740";

        Blockchain blockchain = null;
        Postchain postchain = new Postchain(NODEURL);
        yield return postchain.Blockchain(CHAINID, (Blockchain _blockchain) => blockchain = _blockchain, DefaultErrorHandler);

        Assert.NotNull(blockchain);
    }
}
