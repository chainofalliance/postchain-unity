using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class AssetBalanceTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { }
    private void EmptyCallback() { }

    [UnityTest]
    public IEnumerator AssetBalanceTestRun()
    {
        yield return SetupBlockchain();

        Asset asset1 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset1 = _asset);
        Asset asset2 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset2 = _asset);

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain);
        Account account = null;
        yield return accountBuilder.Build((Account _account) => account = _account);


        yield return AssetBalance.GiveBalance(account.Id, asset1.Id, 10, blockchain, EmptyCallback);
        yield return AssetBalance.GiveBalance(account.Id, asset2.Id, 20, blockchain, EmptyCallback);

        AssetBalance[] balances = null;
        yield return AssetBalance.GetByAccountId(account.Id, blockchain, (AssetBalance[] _balances) => balances = _balances);

        Assert.AreEqual(2, balances.Length);
    }
}
