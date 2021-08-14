using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System.Linq;

public class AssetTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { }
    private void EmptyCallback() { }

    // should be successfully registered
    [UnityTest]
    public IEnumerator AssetTestRun1()
    {
        yield return SetupBlockchain();

        Asset asset = null;
        yield return Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain,
            (Asset _asset) => asset = _asset
        );

        Assert.NotNull(asset);
    }

    // should be returned when queried by name
    [UnityTest]
    public IEnumerator AssetTestRun2()
    {
        yield return SetupBlockchain();

        var assetName = TestUtil.GenerateAssetName();

        Asset asset = null;
        yield return Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain,
            (Asset _asset) => asset = _asset
        );

        Asset[] assets = null;
        yield return Asset.GetByName(assetName, blockchain, (Asset[] _assets) => assets = _assets);
        Assert.AreEqual(1, assets.Length);
        Assert.AreEqual(asset.Name, assets[0].Name);
    }

    // should be returned when queried by id
    [UnityTest]
    public IEnumerator AssetTestRun3()
    {
        yield return SetupBlockchain();

        var assetName = TestUtil.GenerateAssetName();
        var testChainId = TestUtil.GenerateId();
        var assetId = Chromia.Postchain.Client.PostchainUtil.HashGTV(new List<object>() { assetName, testChainId }.ToArray());

        Asset asset = null;
        yield return Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain,
            (Asset _asset) => asset = _asset
        );

        Asset expectedAsset = null;
        yield return Asset.GetById(Util.ByteArrayToString(assetId), blockchain, (Asset _asset) => expectedAsset = _asset);

        Assert.AreEqual(assetName, expectedAsset.Name);
        Assert.AreEqual(assetId, expectedAsset.Id);
        Assert.AreEqual(testChainId, expectedAsset.IssuingChainRid);
    }

    // should return all the assets registered
    [UnityTest]
    public IEnumerator AssetTestRun4()
    {
        yield return SetupBlockchain();
        Asset asset1 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset1 = _asset);
        Asset asset2 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset2 = _asset);
        Asset asset3 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset3 = _asset);

        Asset[] expectedAsset = null;

        yield return Asset.GetAssets(blockchain, (Asset[] _assets) => expectedAsset = _assets);
        var assetNames = expectedAsset.Select(elem => elem.Name).ToList();
        Assert.Contains(asset1.Name, assetNames);
        Assert.Contains(asset2.Name, assetNames);
        Assert.Contains(asset3.Name, assetNames);
    }
}
