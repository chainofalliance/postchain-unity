using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using System;

public class Register
{
    private string _baseURL = "http://localhost:7740";
    private string _blockchainRID = "1A3A5B4C919798B52292094185B37E71898CC245FA9F0AC51A33B473150FE889";
    private Blockchain _blockchain;

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator RegisterWithEnumeratorPasses()
    {
        Postchain postchain = new Postchain(_baseURL);
        yield return postchain.Blockchain<Blockchain>(_blockchainRID,
        (Blockchain _blockchain) =>
        {
            this._blockchain = _blockchain;
        }, (string error) =>
        {
            throw new Exception(error);
        });
    }
}
